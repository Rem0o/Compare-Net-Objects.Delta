using KellermanSoftware.CompareNetObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CompareNetObjects.Delta
{
    public static class Extensions
    {
        public static ComparisonResult<T> Compare<T>(this ICompareLogic compareLogic, T expectedObject, T actualObject)
        {
            Validate(compareLogic.Config);

            ComparisonResult comparisonResult = compareLogic.Compare(expectedObject, actualObject);
            return new ComparisonResult<T>(comparisonResult);
        }

        public static void ActualToExpected<T>(this ComparisonResult<T> comparisonResult, T obj)
        {
            comparisonResult.GetActualToExpected().Apply(obj);
        }

        public static Delta<T> GetActualToExpected<T>(this ComparisonResult<T> comparisonResult)
        {
            return comparisonResult.Differences
                .Select(GetSingleActualToExpected<T>)
                .Aggregate((combinedDelta, delta) => combinedDelta.Merge(delta));
        }

        public static void ApplyExpectedToActualDelta<T>(this ComparisonResult<T> comparisonResult, T obj)
        {
            comparisonResult.GetExpectedToActualDelta().Apply(obj);
        }

        public static Delta<T> GetExpectedToActualDelta<T>(this ComparisonResult<T> comparisonResult)
        {
            return comparisonResult.Differences
                .Select(GetSingleExpectedToActual<T>)
                .Aggregate((combinedDelta, delta) => combinedDelta.Merge(delta));
        }

        private static void Validate(ComparisonConfig comparisonConfig)
        {
            if (comparisonConfig.IgnoreCollectionOrder)
                throw new Exception($"{nameof(comparisonConfig.IgnoreCollectionOrder)} is not supported");
            if (comparisonConfig.IgnoreObjectTypes)
                throw new Exception($"{nameof(comparisonConfig.IgnoreObjectTypes)} is not supported");
        }


        private static Delta<T> GetSingleExpectedToActual<T>(Difference difference)
        {
            if (_expectedToActualCache.TryGetValue(difference.PropertyName, out object obj) && obj is Func<object, Action<T>> generator)
            {
                return new Delta<T>(generator(difference.Object2));
            }
            else
            {
                _expectedToActualCache.Add(difference.PropertyName, GetPropertySetter<T>(difference.Object2, difference.PropertyName));
                return GetSingleExpectedToActual<T>(difference);
            }
        }

        private static Delta<T> GetSingleActualToExpected<T>(Difference difference)
        {
            if (_actualToExpectedCache.TryGetValue(difference.PropertyName, out object obj) && obj is Func<object, Action<T>> generator)
            {
                return new Delta<T>(generator(difference.Object1));
            }
            else
            {
                _actualToExpectedCache.Add(difference.PropertyName, GetPropertySetter<T>(difference.Object1, difference.PropertyName));
                return GetSingleActualToExpected<T>(difference);
            }
        }

        private static Func<object, Action<T>> GetPropertySetter<T>(object propertyValue, string propertyName)
        {
            Action<T> defaultReturn(object v) => o => { };

            if (propertyValue == null)
            {
                return defaultReturn;
            }

            ParameterExpression paramExpression = Expression.Parameter(typeof(T));
            ParameterExpression valueParamExpression = Expression.Parameter(typeof(object));

            string[] subProperties = propertyName.Split('.');
            Expression expression = subProperties.Aggregate(paramExpression as Expression, (result, subProperty) =>
            {
                // indexed property
                if (subProperty.EndsWith("]"))
                {
                    string index = string.Join(string.Empty, subProperty.SkipWhile(c => c != '[').Skip(1).TakeWhile(c => c != ']'));
                    ConstantExpression indexExpression = int.TryParse(index, out var i) ? Expression.Constant(i) : Expression.Constant(index);
                    string currentPropName = string.Join(string.Empty, subProperty.TakeWhile(c => c != '['));
                    MemberExpression indexablePropertyExpression = Expression.Property(result, currentPropName);

                    if (typeof(Array).IsAssignableFrom(indexablePropertyExpression.Type))
                        return Expression.ArrayAccess(
                            indexablePropertyExpression,
                            indexExpression
                        );
                    else
                    {
                        return Expression.Property(indexablePropertyExpression, "item", indexExpression);
                    }

                }
                // non indexed
                else
                {
                    return Expression.PropertyOrField(result, subProperty);
                }
            });

            if (expression is MemberExpression memberExpression && memberExpression.Member is PropertyInfo info && info.CanWrite)
            {
                Expression setProperty = Expression.Assign(memberExpression, Expression.Convert(valueParamExpression, propertyValue.GetType()));
                Expression<Action<T>> funcExpression = Expression.Lambda<Action<T>>(setProperty, paramExpression);
                Expression<Func<object, Action<T>>> funcGeneratorExpression = Expression.Lambda<Func<object, Action<T>>>(
                   funcExpression, valueParamExpression);

                return funcGeneratorExpression.Compile();
            }
            else
            {
                return defaultReturn;
            }
        }

        private static readonly Dictionary<string, object> _expectedToActualCache = new Dictionary<string, object>();
        private static readonly Dictionary<string, object> _actualToExpectedCache = new Dictionary<string, object>();
    }
}

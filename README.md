# Compare-NET-Objects.Delta [![Build Status](https://travis-ci.com/Rem0o/CompareNETObjects.Delta.svg?branch=master)](https://travis-ci.com/Rem0o/CompareNETObjects.Delta)
Let's say we have two versions of a state object:

```c#
State state = new State
{
    CurrentCount = 5,
    Description = "Some important stuff."
};

State otherState = state.Clone();

// change a single property, could be many
otherState.CurrentCount = 6;
```

Now we compare our two states as usual:

```c#
CompareLogic compareLogic = new CompareLogic();
compareLogic.Config.MaxDifferences = int.MaxValue;

ComparisonResult<State> result = compareLogic.Compare<State>(
    expectedObject: state, 
    actualObject: otherState
);

// result.Diffrences.First().PropertyName == "CurrentCount"
```

Let's say we want to be able to undo or redo every change that was made between our two state objects without affecting any other property that could have been changed after the comparison. 

Or maybe we want to apply those changes to any other state object later on. 

To get the deltas, we use the following code:

```c#
/// Is equivalent to:
/// Delta<State>> delta = state => state.CurrentCount = 6;
Delta<State> delta = result.GetExpectedToActualDelta();

/// Is equivalent to:
/// Delta<State>> reversedDelta = state => state.CurrentCount = 5;
Delta<State> reversedDelta = result.GetActualToExpectedDelta();
```
Then we can use them like so:
```c#
/* 
state = {
    CurrentCount: 5,
    Description: "Some important stuff."
}
*/

// change an other property, it will not be affected.
state.Description = "Some even more important stuff.";

delta.Apply(state);

/*
state = {
    CurrentCount: 6,
    Description: "Some even more important stuff."
}
*/

reversedDelta.Apply(state);

/*
state = {
    CurrentCount: 5,
    Description: "Some even more important stuff."
}
*/
```
# Mini Cactpot Solver

This project is a simple .NET Core command line tool for helping maximize MGP gained from the Mini Cactpot game in FFXIV.

You can read about the Cactpot games [here](https://na.finalfantasyxiv.com/lodestone/playguide/contentsguide/goldsaucer/cactpot/).

The project attempts to solve two different problems:

1) Which slot should you uncover next to maximize your chances to maximize your MGP gain.
2) Which line should you select based on what you can see to maximize your MGP gain.

## Slot layout

When asking for a slot, the tool is asking for a number between [0, 8] which corresponds to the following locations on the Cactpot board:

```
0 1 2
3 4 5
6 7 8
```

## How to use

Launch using .NET Core tools:

```
PS C:\users\jsmrc\documents\git\cactpot> dotnet run
```

Then simply follow the prompts. First, you need to enter the slot that is visible on the cactpot board when starting and then the number that is shown. After each move, the tool will show you the current board and recommend a slot to uncover:

```
PS C:\users\jsmrc\documents\git\cactpot> dotnet run
Enter initial slot that is shown: 0
Enter initial value that is shown: 2
Value: 0
2 0 0
0 0 0
0 0 0

Choose one of these cells next: 4
```

Next, you'll simply make the three moves you want to make (either using the suggestions or not) and lastly the game will recommend a line to pick. Below is a sample of the whole tool running:

```
PS C:\users\jsmrc\documents\git\cactpot> dotnet run
Enter initial slot that is shown: 0
Enter initial value that is shown: 2
Value: 0
2 0 0
0 0 0
0 0 0

Choose one of these cells next: 4
Select slot to uncover: 4
Enter uncovered value: 3
Value: 0
2 0 0
0 3 0
0 0 0

Choose one of these cells next: 8
Select slot to uncover: 8
Enter uncovered value: 9
Value: 360
2 0 0
0 3 0
0 0 9

Choose one of these cells next: 2, 6
Select slot to uncover: 2
Enter uncovered value: 6
Value: 720
2 0 6
0 3 0
0 0 9

Choose one of these to get the best outcome:
Third Row
```

Enjoy!
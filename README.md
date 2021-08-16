# Mini Cactpot Solver

This project is a simple .NET 5 WPF tool for helping maximize MGP gained from the Mini Cactpot game in FFXIV.

You can read about the Cactpot games [here](https://na.finalfantasyxiv.com/lodestone/playguide/contentsguide/goldsaucer/cactpot/).

The project attempts to solve two different problems:

1) Which slot should you uncover next to maximize your chances to maximize your MGP gain.
2) Which line should you select based on what you can see to maximize your MGP gain.

## How to use

Launch using [.NET CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/).

```
PS C:\users\jsmrc\documents\git\cactpot> dotnet run
```

The tool will pop up a WPF window. Each button represents one of the scratch ticket slots in-game, which when clicked will allow you to select the value at that location. To start the game, click the slot which is revealed on your in-game cactpot card and give it a value.

![initial state](https://github.com/jsmrcina/cactpot/blob/main/readme-images/initial_state.png?raw=true)

The board will then turn one or more slots bronze, which are the recommended slots to reveal for your next turn:

![turn 1](https://github.com/jsmrcina/cactpot/blob/main/readme-images/turn_1.png?raw=true)

Repeat this until the game is finished and the solver recommends a row/column/diagonal to use to maximize your MGP winnings:

![game over](https://github.com/jsmrcina/cactpot/blob/main/readme-images/gameover.png?raw=true)

Enjoy!
# Chess Engine & Bot

A chess engine built from scratch and integrated into Unity to create a fully playable chess bot with positional evaluation.

🎮 [Play the demo](https://jakejacobi.itch.io/chess)

## Overview

This project implements a chess engine capable of evaluating positions and selecting strong moves in real time. The engine was integrated into a Unity front end, allowing the bot to be played against directly through a graphical interface.

## Features

- **Minimax algorithm** with **alpha-beta pruning** to efficiently search the game tree while discarding branches that won't affect the final decision
- **Quiescence search** to avoid the "horizon effect" by extending search depth in volatile positions (e.g. mid-capture sequences) until the position stabilizes
- **Positional evaluation function** that scores board states beyond simple material count
- Full integration with Unity for move input, board rendering, and game state management

## How It Works

1. The engine generates all legal moves for the current position.
2. Minimax explores the resulting game tree to a fixed depth, alternating between maximizing and minimizing player perspectives.
3. Alpha-beta pruning cuts off branches that can't influence the final decision, significantly reducing the number of positions evaluated.
4. When the search reaches its depth limit in a "noisy" position (e.g. an ongoing capture sequence), quiescence search extends the evaluation further to avoid misjudging the position.
5. The evaluation function scores the resulting positions, and the engine selects the move with the best outcome for the bot.

## Tech Stack

- **C++** — core engine logic (search, evaluation)
- **C#** — Unity integration and gameplay logic
- **Unity** — game front end, board rendering, and user interaction

## Status

This repository contains source recovered from a compiled build. Some project files (Unity scenes, prefabs, assets) may not be included — see repo contents for what's currently available.

## Author

Jake Jacobi — [GitHub](https://github.com/JakeJacobi14) · [LinkedIn](https://www.linkedin.com/in/jakejacobi/)

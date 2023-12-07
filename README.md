# Maze Solver


There are a few gifs in readme, it may take some time to load them.


## Project structure
Project made using Unity DOTS and consist of 2 parts: [maze generation](#maze-generation) and [maze solving](#maze-solving).


## Maze Generation


Maze generated in few steps:
1. Using Recursive Backtracking Algorithm [global cells map](./Assets/Scenes/MazeGeneration/Systems/GenerateMazeSystem.cs) is filled with "roads".
1. Based on generated "roads" map walls are instantiated.
1. Generated start and finish of maze.


![gif](./ReadmeAssets/MazeGeneration.gif)


## Maze Solving


Maze solved by training via Q-learning algorithm model.
Training process consist of several steps:
1. Each training session begins with a new episode.
1. [Episode](./Assets/Scenes/MazeSolver/Episode/EpisodeSystem.cs) spawns new Explorers and Exploiters.
1. [Explorers](./Assets/Scenes/MazeSolver/Exploration/ExplorationSystem.cs) (small white balls) explore maze to find where finish is and mark each road with best possible action. Explorers took action randomly including bumping into a wall.
1. [Exploiters](./Assets/Scenes/MazeSolver/Exploitation/ExploitationSystem.cs) (bigger blue balls) exploit the best path to solve the maze.

Explorers can be improved by introducing other policies like ϵ-Greedy Exploration but it's off topic for this project.

Below is a demonstration of training with debug text indicating best action for each cell with its q-value.

![gif](./ReadmeAssets/Solving.gif)

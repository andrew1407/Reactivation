# Reactivation

<p align="center">
  <a href="https://andrew1407.github.io/Reactivation/">
    <img src="./Assets/Images/Icons/icon.png" width="140" alt="Reactivation Logo" />
  </a>
</p>

[![Tests](https://github.com/andrew1407/Reactivation/actions/workflows/main.yml/badge.svg)](https://github.com/andrew1407/Reactivation/actions/workflows/main.yml)
[![Game packages](https://github.com/andrew1407/Reactivation/actions/workflows/packages.yml/badge.svg)](https://github.com/andrew1407/Reactivation/actions/workflows/packages.yml)
[![GitHub Pages](https://github.com/andrew1407/Reactivation/actions/workflows/pages.yml/badge.svg)](https://github.com/andrew1407/Reactivation/actions/workflows/pages.yml)

## Description

A third-person shooter made with Unity. The game collects player's metrics (during gameplay) to affect on weapon damage modifiers based on their play style, strategy (activity of usage). The project uses Unity's **Inference Engine** (formerly Barracuda) to load a runtime model of ONNX format to operate input/output data (gather and predict values). It is a practical part of the **[bachelor's work](https://ela.kpi.ua/items/f57022f0-be84-4b3d-aa0e-f38f550e1ee1)**.

Neural network model architecture (**[here's](/NeuralNetwork/damage_modifiers.ipynb)** a Python notebook):

![Neural network model](./Doc/Resources/Diagrams/neural-model.png)

Scene objects nesting structure on the main map (level):

![Scene objects](./Doc/Resources/Diagrams/game-objects.png)

Class diagram for main clases in the project:

![Class diagram](./Doc/Resources/Diagrams/class-diagram.png)

## Gameplay

![Gameplay 1](./Doc/Resources/Gameplay/gameplay-1.png)

![Gameplay 2](./Doc/Resources/Gameplay/gameplay-2.png)

![Gameplay 3](./Doc/Resources/Gameplay/gameplay-3.png)

![Gameplay 4](./Doc/Resources/Gameplay/gameplay-4.png)

![Gameplay 5](./Doc/Resources/Gameplay/gameplay-5.png)

![Gameplay 6](./Doc/Resources/Gameplay/gameplay-6.png)

## Testing

Edit mode tests cover the gameplay logic (damage modifiers, kill stats, waves and timer, health, the object pool, camera input, stuck ammo); a play mode smoke test loads the main scene and checks the player spawns. Run them from **Window → General → Test Runner** or headless:

```
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults editmode.xml
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults playmode.xml
```

## Building locally

Use the **Build** menu in the editor, or [build.ps1](build.ps1) to build with the project's Unity version in batch mode and zip the results into `Builds/` (close the project in the editor first):

```
./build.ps1                                        # Windows + WebGL
./build.ps1 -Platforms Windows,Linux,macOS,WebGL   # every platform whose build support module is installed
```

The web build runs on WebGPU where the browser supports it and falls back to WebGL 2 otherwise; serve `Builds/WebGL` over HTTP (opening `index.html` from disk won't load it).

The machete model is kept as an FBX under `Assets/`; its editable Blender source lives in [Models/](Models) so builds don't need Blender installed.

## Releases

Pushing a version tag (`git tag v1.0.0 && git push origin v1.0.0`) builds Windows, Linux, macOS and Web packages and attaches them to the GitHub release. The web build is also deployed to [GitHub Pages](https://andrew1407.github.io/Reactivation/) on every push to `main` that changes the game.

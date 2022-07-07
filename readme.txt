** How to get started

NOTE: The simulator uses the Universal Render Pipeline. So you can best start with a new URP project.

- Add the following lines to your manifest to include this package and its dependencies:
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.unidice.sdk": "https://github.com/ExtraNiceBV/Unidice-SDK.git",
    "com.unidice.simulator": "https://github.com/ExtraNiceBV/Unidice-Simulator.git",
  
- Import the simulator and the template "samples" from the package manager. You can ignore conflicting GUID warnings (it is resolved correctly).

- Drag the Template folder from Samples to your Asset folder in the project view. You can rename and change it later as much as you wish.

- Add the Loader scene from the template as your first build scene, and the simulator scene from the samples folder as the second.

- Add your own (or one of the template scenes) to the build scenes and open it.

- Click "Load simulator" in the hierarchy to load the simulator scene with it.

UPDATING: If you want to update to the latest version of the SDK or Simulator, delete the packages-lock.json file from your Packages folder. Unity doesn't automatically update git packages, because... Unity.
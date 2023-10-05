# UnityTools
These are some tools I created to help me work on my Unity projects.

Feel free to learn and take from this project what you need.

Features included:
- Packages
    - DevConsole: A console GUI element that shows log messages and accepts input to execute commands.
    - LocalizationManager: Translates text components into multiple languages.
    - VisualPath: Creates a visual path between two points. Includes a shader that animates the path.
- Plugins
    - websocket: Provides a connection via websocket.
- Scripts
    - Animations/EquipmentParenter: Parents clothing to animate with a character.
    - Animations/GenericAnimationEvents: Sends events from the animator state machine to the gameobject component "AnimationEventReceiver".
    - Animations/AnimationEventReceiver: A component for a gameobject with an animator. receives events from "GenericAnimationEvents"-class.
    - Audio/SoundManager: Provides an easy way to reference, instantiate, and manage audio assets.
    - CameraTools/CameraControllerTopDown: Moves the camera in all directions within set limits. Supports mouse, keyboard, and touchscreen.
    - DevTools/DevConsole: see "DevConsole" package.
    - DevTools/GameStatistics: Prints FPS and memory usage to a text component.
    - DevTools/MiscellaneousDevTools: Prohibits Unity from focusing on the game window after pressing start.
    - Effects/Effect: Manages a particle system.
    - Effects/EffectsManager: Manages and caches effects.
    - Effects/VisualPathManager: See "VisualPath" package.
    - GameObjects/GameObjectCache: A generic cache class for a gameobject prefab.
    - GameObjects/ObjectPlacer: Previews gameobjects that can be built by the player.
    - Localization/LocalizationManager: Provides texts for different languages based on xml-files that are named in a .Net-TwoLetterISOLanguageName pattern.
    - Localization/LocalizationText: Sets text for TMPro.TMP_Text for the language in "LocalizationManager".
    - Networking/Analytics: Formats and sends analytics-data to a server via TCP or websocket. Manages connection.
    - Networking/NetworkConnection: Abstract parent class for connections (TCP/websocket).
    - Networking/TCPConnection: Manages a TCP-connection.
    - Networking/WebSocketConnection: Manages a websocket-connection. Only in webGL. Uses plugin: websocket.
    - Pathfinding/BasicAStar: Provides A* pathfinding.
    - Pathfinding/PathfindingObstacle: A pathfinding obstacle visible in the scene-view. snaps to a grid.
    - Pathfinding/Rasterizer: Creates a grid. Manages grid tiles and pathfinding.
    - Random/RandomValuesSeed: Provides random values based on perlin noise. provides different data-types (float, int, bool).
    - Saving/SaveFileManager: Manages save files on a key-value basis. files are in XML format.
    - TimeTools/WorldTime: Replaces the unity time class for world time. Provides a separate time scale and an option to pause time.
    - Tools/HierarchyTools: Provides a function to find a child by name in a gameobject's hierarchy.
    - Triggers/GenericTriggerEvents: Exposed the unity-events for a trigger (OnTriggerEnter/OnTriggerExit) as events for other classes to use.
    - UI/ChildrenColoringButton: A button that changes the color of all its children with its own color.
    - UI/CursorRayCastTarget: Provides functions to detect if the cursor is within a collider. relies on the UIManager class.
    - UI/ExtendedButton: base class for other buttons. Provides additional events.
    - UI/FadeOutButton: A button that will move towards an edge. Every pixel over the edge will get hidden. The button will move over the edge and finally disappear.
    - UI/FadingImage: Alternates an image's alpha between 0 and m_maxAlpha.
    - UI/FadingTextWorld: A world text that moves upwards and destroys itself if it is active for longer than m_lifeTime.
    - UI/HyperlinkField: A component that can be put on top of an UI element. If it is clicked, a URL will be opened. Also, the text gets highlighted if the cursor is over the UI.
    - UI/MovingColorImage: Moves a vertical line of color horizontally through the image. A special shader needs to be used: "UiColorFading"
    - UI/StableButton: A button that can be toggled by mouse0 or temporarily pressed by mouse1. depends on guimanager to send cursor enter.
    - UI/UIManager: Sends cursor-enter messages to different gameobjects.
    - UI/UIScaleEffect: A scale effect on an UI element. It scales the element up for m_time to m_maxSize. Once m_maxSize is reached, it reverts to its normal size.
    - UI/UITools: Some functions for transforming rects.

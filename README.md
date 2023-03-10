# Mirror_MultiplayerFps_Demo
>This project is a Unity3D multiplayer fps demo, but it has not finished yet. The project plan to have 3 scenes, welcome scene, room scene and main scene.
> - **welcome scene `(finished)`** A start interface, there are serveral buttons, which allow player to choose to player as a host or client according to customized ip and port, and some other functions, like setting or developer information.
> - **room scene `(finished,bug fixing)`** when player enter to this scene, a ready interface, they can change change their user name and ready state, and these states are visible for other roommates. while all players are ready, the host player(not the real host, just the player who enter first) can start game.
> - **main scene `(finished 40%)`** player could controll a 1st person character to do basic movement, and some complex operation, like grappling. For weapon system, the character can pick up or drop down weapons (guns and knife), and the weapon can attack other target. the whole process is also visible for other roommates.

## Project Setting
> - **unity Editor** 2021.3.14f1 (lts), do not recommand other higher version editor, it would be crashed while download urp.
> - **WAN game** currently the game does not have public network servers, if want to play in WAN(wide area network), please use UDP NAT traversal, like sakuraFrp.
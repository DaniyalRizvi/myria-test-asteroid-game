How to play:
- Set a room id before starting a match
- Keys ‘WASD’ for spaceship rotation
- spacebar for acceleration
- while pressing space you can only rotate using ‘W’ & ’S’ for clockwise and
antilock wise rotation
- while !pressing space you can only rotate using ‘A’ & ‘D’ for clockwise and
anticlockwise rotation
- left mouse click for firing
Additional Info:
- players are colour coded
- player limit is set to 4
- player stats are cooler coded as well
- Bonus work is not included
What I followed:
- I followed Fusion Manual (100 series). I am basically experienced with PUN2 & Quantum that is why I had to go through 100 series to know about the scriptable use of fusion.
Technical Issues Faced:
- Since I am new to fusion understanding, the biggest issue I faced is that pun2 and fusion has totally different components, sync is different as well as network objects & components.
- I used the latest SDK of fusion but I guess it’s unstable right now because while I was using rigidbody2d I was getting exceptions which I eliminated by reverting to the previous version (1.1.2.546)
- In the latest version the fusion physics library was going haywire upon even initialising the fusion and the rigid body exception was fixed just by reverting to previous version of fusion and the same code worked fine.
- There’s not much documentation available to explore the components. One of the major issue I faced for position syncing and teleportation of objects if they go off screen was their transform values. I was using NetworkTransform and NetworkRigidBody2D component on my object whereas it is only recommended to use one of them since they both sync the positioning of objects over network otherwise they conflict with each other
- There are other things as well to note about transform sync is that you have to select Interpolation as Predicted in NetworkTransform and always feed the new position to NetworkTransform in order to position your object locally and over the network.
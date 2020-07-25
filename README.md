# AEC Hackathon Online 2020 Project

This is a continuation and expansion of existing work from a [previous hackathon](https://github.com/LMA-Studio/enghackathon2020)

Below you will find details pertaining to the building and running of the solution

### Building

- StreamVR.Revit project targets `.NET 4.7.2`
- StreamVR.Common builds against `.NETStandard 2.0`
- StreamVR.ModelServer utilizes `Node.js v14.5`

### Library Dependencies

- Newtonsoft.Json [2.0.1]
  - Needs [JSON .NET For Unity](https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347) due to running on mobile device (Oculus Quest)
- Revit.RevitAPI.x64 [2019.0.0]
- Revit.RevitAPIUI.x64 [2019.0.0]
- [NATS.Client](https://github.com/nats-io/nats.net) [0.10.1]
  - Build targeting .NET Standard 2.0 can be found in StreamVR.Common/Libs directory

### Running

External NATS service and Model-Server must be running. This is not being shipped with the add-in as there is no guarantee of administrative privileges being granted to the user account running Revit so an external service is safer.

#### AWS EC2 Example Deployment

- Launch instance. Can be small for most purposes (e.g. t3a.nano)
- SSH into machine
- SCP `StreamVR.ModelServer` or pull from GitHub source
- [Install Node.js](https://docs.aws.amazon.com/sdk-for-javascript/v2/developer-guide/setting-up-node-on-ec2-instance.html)
- [Install NATS](https://docs.nats.io/nats-server/installation#downloading-a-release-build)
- `cd` into the `StreamVR.ModelServer` directory and run `npm install`
- Run the Node.js server in background. (e.g. `node index.js > node.log 2> node.error.log &`)
- Run the NATS bus in background. (e.g. `nats-server 2> nats.log &`)
- Ensure that your security group allows incoming traffic to ports `4222` (NATS) and `8080` (Node.js)
- Create a DNS `A` record to point to the Public IP address of this instance (recommended)

# Inspiration

Having full-scale mockups allow owners to understand how their building will function in a way that renderings can't convey. Currently, this requires additional cost to everyone due to the time it takes to produce VR experiences on an individual project basis.

## What it does

Our solution enables bi-directional streaming of data between a Revit model and our VR application. This allows client, architects, and contractors to edit their Revit models in VR and synchronize those changes back to Revit in real-time.

[Teaser Video](https://youtu.be/Qn0iEui4PNk)

[Demo Video](https://youtu.be/HLJG9sCA2FI)

## How we built it

There are 4 moving parts to our solution

#### Message Bus

The message bus is the simplest component of our solution. We are using a [NATS](https://nats.io) server to allow for real-time communication between our other system components. This is a light-weight bus that separates traffic into queues and channels that can be published and subscribed to.

#### Model Server

The model server allows for the storage of exported Revit family geometry and materials. It acts as a simple API: the Revit add-in can `POST` OBJ and material data and the VR application can `GET` them as needed. This server also comes with a simple web portal to view and manage cached data.

#### Revit Add-in

This add-in allows the Revit model to connect to the message bus and receive commands from a connected StreamVR application. Upon receiving a request or command, the add-in executes a subroutine. These processes primarily include Getting, Setting, Painting, and Deleting. Also part of this add-in is the ability to export Family geometry and Material textures to a caching model-server that the VR application will later access.

#### VR Application

The VR application is where BIM data are rendered and interacted with. It first connects with the message bus and the Revit add-in using the provided configuration parameters. It then queries Revit for structural geometry such as walls, floors, and ceilings along with metadata pertaining to Families and Materials. After retrieving these data, the application then renders the VR space, downloading OBJ models and textures from the model-server as needed.

## Challenges we ran into

We certainly ran into plenty of challenges along the way. One of our biggest hurdles was handling hosted families. We encountered problems with synchronizing and updating placements when these families included sub-components. Another challenge we faced was the Unity3D learning curve as we came into this hackathon as relative novices. 

## Accomplishments that we're proud of

We are proud of all the progress we made during this Hackathon. Some noteworthy achievements include our dynamic menu loading and our Revit Family caching and retrieval which allows for the use of any Revit Family in VR.

## What we learned

Along the way, we learned many deeper features about Unity3D including how Coroutines work and some of the finer points regarding shaders and post-processing.

## What's next for StreamVR

In the future, we would like to enhance our dynamic lighting and material rendering. We would also like to solve the challenges we faced with hosted families and allow for placing them and moving them between hosts. Additionally, we would like to incorporate the option to move non-family geometry such as walls, floor, and ceilings. Finally, we built our VR app using Unity's XR toolkit which allows for future adoption by other hardware including AR headsets.


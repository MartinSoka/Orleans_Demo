# Orleans_Demo
MS Orleans Quickstart Demo using ASP.NET Core 7.0 Minimal APIs. 

Users can submit a full URL to the app, which will return a shortened version they can share with others, who will then be redirected to the original site. 
The app uses Orleans grains and silos to manage state in a distributed manner to allow for scalability and resiliency.

In prod env i'm using Azurite Emulator for storing the orleans cluster data and grains to simulate real world.

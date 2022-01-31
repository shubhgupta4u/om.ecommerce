Task A: Config Server

Step 1: Start config Server
	-> mongod --config ./config/mongod_cnfsvr.conf

Step 2: Connect to mongo shell and initialize relication set for config Server - (One time activity)
	-> mongo --port 27019
	-> rs.initiate( { _id: "csRS", configsvr: true, members: [ { _id: 0, host: "localhost:27019" } ] } )

Task B: Mongo DB Shard Cluster 1
Step 1: Start Mongo DB Shard Cluster 1
	-> mongod --config ./config/mongod1.conf

Step 2: Connect to mongo shell and initialize relication set for Mongo DB Shard Cluster 1 - (One time activity)
	-> mongo --port 27017
	-> rs.initiate( { _id: "shardA",members: [ { _id: 0, host: "localhost:27017" } ] } )

Task C: Mongo DB Shard Cluster 2
Step 1: Start Mongo DB Shard Cluster 2
	-> mongod --config ./config/mongod2.conf

Step 2: Connect to mongo shell and initialize relication set for Mongo DB Shard Cluster 2 - (One time activity)
	-> mongo --port 27018
	-> rs.initiate( { _id: "shardB",members: [ { _id: 0, host: "localhost:27018" } ] } )

Task D: Mongo Sharding instance
Step 1: Start Mongos instance
	-> mongos --config ./config/mongos.conf

Step 2: Connect to mongo shell and initialize sharding set for Mongo Shard Instance - (One time activity)
	-> mongo --port 27001
	-> sh.addShard( "shardA/localhost:27017" )
	-> sh.addShard( "shardB/localhost:27018" )
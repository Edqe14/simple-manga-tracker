using MongoDB.Driver;
using System.Reflection;

string workPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception("Couldn't find current path");

ConfigManager manager = new ConfigManager(workPath);
DirectoryReader list = new DirectoryReader(workPath);

MongoClient mongoClient = new MongoClient(manager.config["database"]["uri"] ?? "mongodb://localhost:27017");
var database = mongoClient.GetDatabase(manager.config["database"]["name"] ?? "simple-manga-tracker");

Renderer renderer = new Renderer(database, list, manager);
renderer.Start();
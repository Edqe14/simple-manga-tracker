using MongoDB.Driver;
using Pastel;
using System.Drawing;
using MongoDB.Bson;

class Renderer {
  public readonly string FinishedIndicator = $"[{"✓".Pastel(Color.Green)}]".Pastel(Color.DarkGray);
  public readonly IMongoDatabase database;
  public readonly DirectoryReader list;
  public readonly ConfigManager manager;
  public int level = 0;
  public int position = 0;
  public dynamic? activeObject = null;
  private IMongoCollection<BsonDocument> collection;

  public Renderer(IMongoDatabase rDatabase, DirectoryReader rList, ConfigManager rManager) {
    database = rDatabase;
    list = rList;
    manager = rManager;

    collection = database.GetCollection<BsonDocument>("finished");
  }

  public void Start() {
    while(true) {
      ResetConsole();

      (string name, bool read)[] lst = GetList();

      Draw(lst);

      bool next = true;
      var key = Console.ReadKey();
      switch(key.Key) {
        case ConsoleKey.DownArrow: {
          position = Math.Clamp(position + 1, 0, Math.Clamp(lst.Length - 1, 0, int.MaxValue));
          break;
        }

        case ConsoleKey.UpArrow: {
          position = Math.Clamp(position - 1, 0, Math.Clamp(lst.Length - 1, 0, int.MaxValue));
          break;
        }

        case ConsoleKey.Enter: {
          if (level == 2) break;

          activeObject = GetActiveObject();
          position = 0;
          level = Math.Clamp(level + 1, 0, 2);

          break;
        }

        case ConsoleKey.Escape: {
          level = Math.Clamp(level - 1, 0, 2);
          position = 0;
          activeObject = GetActiveObject(-1);

          break;
        }

        case ConsoleKey.Q: {
          next = false;
          break;
        }

        case ConsoleKey.Spacebar: {
          var currentObject = GetActiveObject();
          string? hash = currentObject?.hash;

          if (currentObject is not null && hash is not null) {
            var document = GetDocument(hash);

            if (document == null) {
              SetDocument(hash, true);
              break;
            }

            var filter = Builders<BsonDocument>.Filter.Eq("hash", hash);
            var update = Builders<BsonDocument>.Update.Set("read", !document["read"].ToBoolean());

            collection.UpdateOne(filter, update);
          }

          break;
        }
      }

      if (!next) break;
    }

    ResetConsole();
  }

  private void Draw((string name, bool read)[] lst) {
    Console.WriteLine(Utils.PastelWithGradient(GetLevelName(), Color.DeepSkyBlue, Color.LightBlue));
    Console.WriteLine();

    if (lst.Length == 0) {
      Console.WriteLine("There's nothing :(");
      return;
    }

    foreach(var entry in lst.Select((str, index) => (str.name, str.read, index))) {
      string padding = new String(' ', Console.WindowWidth - entry.name.Length - 1 - (entry.read ? 3 : 0));
      string name = $"{(entry.read ? FinishedIndicator : "")} {entry.name}{padding}";
      int index = entry.index;

      if (position == index) name = name.Pastel(Color.Red).PastelBg(Color.White);

      Console.WriteLine(name);
    }

    Console.WriteLine();
    Console.WriteLine(GetHelpText());
  }

  public string GetHelpText() {
    Color highlight = Color.Orange;
    Color color = Color.Gray;

    string[] texts = {
      $"{"Esc".Pastel(highlight)} - Go up a level".Pastel(color),
      $"{"Enter".Pastel(highlight)} - Go down a level".Pastel(color),
      $"{"Space".Pastel(highlight)} - Toggle mark".Pastel(color),
      $"{"q".Pastel(highlight)} - Quit".Pastel(color)
    };

    return string.Join(" | ", texts);
  }

  public (string name, bool read)[] GetList() {
    switch (level) {
      case 0: return list.titles.Select(title => (title.name, IsFinished(title.hash))).ToArray();

      case 1: return (activeObject as Title)!.volumes.Select(volume => (volume.name, IsFinished(volume.hash))).ToArray();

      case 2: return (activeObject as Volume)!.chapters.Select(chap => ($"{chap.name} {$"[{chap.pages.Length} page(s)]".Pastel(Color.Fuchsia)}", IsFinished(chap.hash))).ToArray();

      default: return new (string ,bool)[0];
    }
  }

  public BsonDocument SetDocument(string hash, bool read = false) {
    var document = new BsonDocument{
      { "hash", hash },
      { "read", read }
    };

    collection.InsertOne(document);

    return document;
  }

  public BsonDocument? GetDocument(string hash) {
    return collection.Find(Builders<BsonDocument>.Filter.Eq("hash", hash)).FirstOrDefault() ?? null;
  }

  public bool IsFinished(string hash) {
    var document = GetDocument(hash);
    if (document == null) return false;

    return document["read"].ToBoolean();
  }

  public dynamic? GetActiveObject(int direction = 1) {
    switch (level){
      case 0: {
        if (direction == -1) return null;

        return list.titles[position];
      };

      case 1: {
        if (direction == -1) return list.titles[position];

        return activeObject!.volumes[position];
      }

      case 2: {
        if (direction == -1) return activeObject!.volumes[position];

        return activeObject!.chapters[position];
      }

      default: return null;
    }
  }

  public string GetLevelName() {
    switch (level) {
      // Titles
      case 0: return "░▀█▀░▀█▀░▀█▀░█░░░█▀▀░█▀▀\n░░█░░░█░░░█░░█░░░█▀▀░▀▀█\n░░▀░░▀▀▀░░▀░░▀▀▀░▀▀▀░▀▀▀";
      // Volumes
      case 1: return "░█░█░█▀█░█░░░█░█░█▄█░█▀▀░█▀▀\n░█░█░█░█░█░░░█░█░█░█░█▀▀░▀▀█\n░░▀░░▀▀▀░▀▀▀░▀▀▀░▀░▀░▀▀▀░▀▀▀";
      // Chapters
      case 2: return "░█▀▀░█░█░█▀█░█▀█░▀█▀░█▀▀░█▀▄░█▀▀\n░█░░░█▀█░█▀█░█▀▀░░█░░█▀▀░█▀▄░▀▀█\n░▀▀▀░▀░▀░▀░▀░▀░░░░▀░░▀▀▀░▀░▀░▀▀▀";
      default: return "Unknown";
    }
  }

  public int GetActiveObjectType() {
    if (activeObject is Title) return 0;
    if (activeObject is Volume) return 1;
    if (activeObject is Chapter) return 2;

    return -1;
  }

  public void ResetConsole() {
    Console.CursorVisible = false;
    Console.SetCursorPosition(0, 0);
    Console.Clear();
  }
}
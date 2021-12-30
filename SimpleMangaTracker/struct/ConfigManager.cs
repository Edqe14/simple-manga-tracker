using Tommy;

public class ConfigManager {
  public string configPath;
  public TomlTable config;

  public ConfigManager(string workPath) {
    configPath = Path.Join(workPath, "config.toml");
    config = Parse();
  }

  public TomlTable Parse() {
    TomlTable table;
    if (!File.Exists(configPath)) {
      table = CreateDefault();

      using(StreamWriter writer = File.CreateText(configPath)) {
        table.WriteTo(writer);
        writer.Flush();
      }
    } else {
      using(StreamReader reader = File.OpenText(configPath)) {
        table = TOML.Parse(reader);
      }
    }

    return table;
  }

  public TomlTable CreateDefault() {
    TomlTable table = new TomlTable{
      ["database"] = {
        ["uri"] = "mongodb://localhost:27017",
        ["name"] = "simple-manga-tracker"
      }
    };

    return table;
  }
}
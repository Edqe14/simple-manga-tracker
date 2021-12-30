class Title {
  public string hash;
  public string name;
  public string path;
  public Volume[] volumes = {};

  public Title(string titlePath) {
    if (!Directory.Exists(titlePath)) throw new Exception("Invalid title path");

    name = new DirectoryInfo(titlePath).Name;
    hash = Utils.CreateMD5Hash(name);
    path = titlePath;
    volumes = Directory.GetDirectories(path)
      .Select(ph => new Volume(ph, this))
      .ToArray();
  }
}
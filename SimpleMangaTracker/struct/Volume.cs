class Volume {
  public string hash;
  public string name;
  public string path;
  public Chapter[] chapters = {};
  public Title title;

  public Volume(string volumePath, Title vTitle) {
    if (!Directory.Exists(volumePath)) throw new Exception("Invalid volume path");

    title = vTitle;
    name = new DirectoryInfo(volumePath).Name;
    hash = Utils.CreateMD5Hash(title.name + name);
    path = volumePath;
    chapters = Directory.GetDirectories(path)
      .Select(ph => new Chapter(ph, this))
      .ToArray();
  }
}
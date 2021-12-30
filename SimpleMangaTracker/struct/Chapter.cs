class Chapter {
  public string hash;
  public string name;
  public string path;
  public string[] pages = {};
  public Volume volume;

  public Chapter(string chapterPath, Volume cVolume) {
    if (!Directory.Exists(chapterPath)) throw new Exception("Invalid volume path");

    volume = cVolume;
    name = new DirectoryInfo(chapterPath).Name;
    hash = Utils.CreateMD5Hash(volume.name + name);
    path = chapterPath;
    pages = Directory.GetFiles(path)
      .Select(ph => new FileInfo(ph).Name)
      .ToArray();
  }
}
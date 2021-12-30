class DirectoryReader {
  public string dirPath;

  public Title[] titles = {};

  public DirectoryReader(string workPath) {
    dirPath = Path.Join(workPath, "mangas");

    if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

    titles = Directory.GetDirectories(dirPath)
      .Select(path => new Title(path))
      .ToArray();
  }
}
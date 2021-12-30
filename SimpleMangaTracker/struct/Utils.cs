using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using Pastel;

// https://github.com/silkfire/Pastel/pull/5/
public class StyleClass<T> : IEquatable<StyleClass<T>>
{
  /// <summary>
  /// The object to be styled.
  /// </summary>
  public T Target { get; set; }

  /// <summary>
  /// The color to be applied to the target.
  /// </summary>
  public Color Color { get; set; }

  /// <summary>
  /// Exposes methods and properties that represent a style classification.
  /// </summary>
  /// <param name="target">The object to be styled.</param>
  /// <param name="color">The color to be applied to the target.</param>
  public StyleClass(T target, Color color)
  {
    Target = target;
    Color = color;
  }

  public bool Equals(StyleClass<T> other)
  {
    if (other == null)
    {
      return false;
    }

    return Target!.Equals(other.Target) && Color == other.Color;
  }

  public override bool Equals(object obj) => Equals(obj as StyleClass<T>);

  public override int GetHashCode()
  {
    int hash = 163;

    hash *= 79 + Target!.GetHashCode();
    hash *= 79 + Color.GetHashCode();

    return hash;
  }
}

static class Utils
{
  public static string CreateMD5Hash(string input)
  {
    MD5 md5 = MD5.Create();
    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
    byte[] hashBytes = md5.ComputeHash(inputBytes);

    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("X2"));

    return sb.ToString();
  }

  public static List<StyleClass<T>> GenerateGradient<T>(IEnumerable<T> input, Color startColor, Color endColor, int maxColorsInGradient)
  {
    var inputAsList = input.ToList();
    var numberOfGrades = inputAsList.Count / maxColorsInGradient;
    var numberOfGradesRemainder = inputAsList.Count % maxColorsInGradient;

    var gradients = new List<StyleClass<T>>();
    var previousColor = Color.Empty;
    var previousItem = default(T);
    int SetProgressSymmetrically(int remainder) => remainder > 1 ? -1 : 0; // An attempt to make the gradient symmetric in the event that maxColorsInGradient does not divide input.Count evenly.
    int ResetProgressSymmetrically(int progress) => progress == 0 ? -1 : 0; // An attempt to make the gradient symmetric in the event that maxColorsInGradient does not divide input.Count evenly.
    var colorChangeProgress = SetProgressSymmetrically(numberOfGradesRemainder);
    var colorChangeCount = 0;

    bool IsFirstRun(int index) => index == 0;
    bool ShouldChangeColor(int index, int progress, T current, T previous) => (progress > numberOfGrades - 1 && !current.Equals(previous) || IsFirstRun(index));
    bool CanChangeColor(int changeCount) => changeCount < maxColorsInGradient;

    for (var i = 0; i < inputAsList.Count; i++)
    {
      var currentItem = inputAsList[i];
      colorChangeProgress++;

      if (ShouldChangeColor(i, colorChangeProgress, currentItem, previousItem) && CanChangeColor(colorChangeCount))
      {
        previousColor = GetGradientColor(i, startColor, endColor, inputAsList.Count);
        previousItem = currentItem;
        colorChangeProgress = ResetProgressSymmetrically(colorChangeProgress);
        colorChangeCount++;
      }

      gradients.Add(new StyleClass<T>(currentItem, previousColor));
    }

    return gradients;
  }

  private static Color GetGradientColor(int index, Color startColor, Color endColor, int numberOfGrades)
  {
    var numberOfGradesAdjusted = numberOfGrades - 1;

    var rDistance = startColor.R - endColor.R;
    var gDistance = startColor.G - endColor.G;
    var bDistance = startColor.B - endColor.B;

    var r = startColor.R + (-rDistance * ((double)index / numberOfGradesAdjusted));
    var g = startColor.G + (-gDistance * ((double)index / numberOfGradesAdjusted));
    var b = startColor.B + (-bDistance * ((double)index / numberOfGradesAdjusted));

    var graded = Color.FromArgb((int)r, (int)g, (int)b);
    return graded;
  }

  public static string PastelWithGradient(this string input, Color startColor, Color endColor)
  {
    var gradient = GenerateGradient(input, startColor, endColor, input.Length);

    return string.Concat(gradient.Select(x => x.Target.ToString().Pastel(x.Color)));
  }
}
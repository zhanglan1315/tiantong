namespace Tiantong.Wms.Api
{
  public interface IRandom
  {
    string String(int length);

    int Range(int min, int max);
  }
}

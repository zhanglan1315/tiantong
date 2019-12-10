namespace Wcs.Plc
{
  public interface IContainer
  {
    IEvent Event { get; }

    IStateDriver StateDriver { get; }

    IStateManager StateManager { get; }

    IIntervalManager IntervalManager { get; }
  }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Wcs.Plc
{
  using Intervals = Dictionary<int, Interval>;

  public class IntervalManager
  {
    private int _id = 0;

    public Intervals Intervals = new Intervals();

    public IntervalManager()
    {
      var interval = new Interval();
      interval.SetTime(100).SetHandler(() => {
        foreach (var interval in Intervals.Values) {
          var task = interval.WaitAsync();
          if (task.IsCompleted) {
            try {
              task.GetAwaiter().GetResult();
            } catch (Exception e) {
              Stop();
              throw e;
            }
          }
        }
      });
      Add(interval);
    }

    public void Add(Interval interval)
    {
      var id = _id++;
      interval.Id = id;
      Intervals.Add(id, interval);
    }

    public Task Remove(Interval interval)
    {
      var id = interval.Id;
      interval.Stop();
      interval.WaitAsync().ContinueWith(_ => {
        interval.Id = 0;
        Intervals.Remove(id);
      });

      return interval.WaitAsync();
    }

    public bool IsRunning()
    {
      foreach (var interval in Intervals.Values) {
        if (interval.IsRunning()) {
          return true;
        }
      }

      return false;
    }

    public IntervalManager Start()
    {
      foreach (var interval in Intervals.Values) {
        interval.Start();
      }

      return this;
    }

    public IntervalManager Stop()
    {
      foreach (var interval in Intervals.Values) {
        if (interval.IsRunning()) {
          interval.Stop();
        }
      }

      return this;
    }

    public IntervalManager Clear()
    {
      foreach (var interval in Intervals.Values) {
        Remove(interval);
      }

      return this;
    }

    public Task WaitAsync()
    {
      return Task.WhenAll(
        Intervals.Values.Select(i => i.WaitAsync()).ToArray()
      );
    }

    public void Wait()
    {
      WaitAsync().GetAwaiter().GetResult();
    }

    public Task RunAsync()
    {
      return Start().WaitAsync();
    }

    public void Run()
    {
      Start().Wait();
    }
  }
}

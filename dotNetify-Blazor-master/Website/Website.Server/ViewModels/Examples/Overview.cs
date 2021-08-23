﻿using System;
using System.Threading;
using DotNetify;

namespace Website.Server
{
   public class RealTimePush : BaseVM
   {
      private Timer _timer;
      public string Greetings => "Hello World!";
      public DateTime ServerTime => DateTime.Now;

      public RealTimePush()
      {
         _timer = new Timer(state =>
         {
            Changed(nameof(ServerTime));
            PushUpdates();
         }, null, 0, 1000); // every 1000 ms.
      }

      public override void Dispose() => _timer.Dispose();
   }

   public class ServerUpdate : BaseVM
   {
      public class Person
      {
         public string FirstName { get; set; }
         public string LastName { get; set; }
      }

      public string Greetings { get; set; } = "Hello World!";

      public void Submit(Person person)
      {
         Greetings = $"Hello {person.FirstName} {person.LastName}!";
         Changed(nameof(Greetings));
      }
   }

   public class TwoWayBinding : BaseVM
   {
      public string Greetings => $"Hello {Name}";

      private string _name = "World";

      public string Name
      {
         get => _name;
         set
         {
            _name = value;
            Changed(nameof(Greetings));
         }
      }
   }
}
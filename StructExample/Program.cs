using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace StructExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var @event = new ValueEvent();
            @event.EventType = EventType.Default;

            BetInfo bet;

            @event.Payload.Bet.Id = DateTime.Now.Ticks;
            @event.Payload.Bet.AccountId = 100002L;
            @event.Payload.Bet.EventId = 100;
            @event.Payload.Bet.MarketId = 20004;
            @event.Payload.Bet.SelectionId = 3;
            @event.Payload.Bet.Side = SideType.Back;
            @event.Payload.Bet.Price = 3.0m;
            @event.Payload.Bet.Stake = 100m;
            @event.Payload.Bet.Created = DateTime.Now;

            var serializedBet = StructMarshal(@event.Payload);
            EventData payload = StructUnmarshal<EventData>(serializedBet, 0);
            
            var sw = Stopwatch.StartNew();
            var iterations = 1000000;
            for (int i = 0; i < iterations; i++)
            {
                var s = StructMarshal(@event.Payload);
            }
            sw.Stop();
            Console.WriteLine($"Marshal struct to bytes {iterations} times, elapsed {sw.Elapsed.Milliseconds} ms.");
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var b = StructUnmarshal<BetInfo>(serializedBet, 0);
            }
            sw.Stop();
            Console.WriteLine($"Unmarshal bytes to struct {iterations} times, elapsed {sw.Elapsed.Milliseconds} ms.");
            
            
            
            Console.WriteLine("Press any key to continue...");
            Console.Read();
        }
        
        /// <summary>
        /// converts byte[] to struct
        /// </summary>
        public static T StructUnmarshal<T>(byte[] data, int position)
        {
            var dataSize = Marshal.SizeOf(typeof(T));
            var structSize = data.Length - position;
            if (dataSize > structSize)
                throw new ArgumentException(
                    $"Not enough data to fill struct. Array length from position: {structSize}, Struct length: {dataSize}");
            T structObj;
            var buffer = Marshal.AllocHGlobal(dataSize);
            try
            {

                Marshal.Copy(data, position, buffer, dataSize);
                structObj = (T)Marshal.PtrToStructure(buffer, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return structObj;
        }

        /// <summary>
        /// converts a struct to byte[]
        /// </summary>
        public static byte[] StructMarshal(object anything)
        {
            var rawSize = Marshal.SizeOf(anything);
            var buffer = Marshal.AllocHGlobal(rawSize);
            var data = new byte[rawSize];
            try
            {
                Marshal.StructureToPtr(anything, buffer, false);
                Marshal.Copy(buffer, data, 0, rawSize);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return data;
        }
    }

    public class ValueEvent
    {
        public EventType EventType;
        public EventData Payload;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct EventData
    {
        [FieldOffset(0)]
        public BetInfo Bet;

        [FieldOffset(0)]
        public MarketInfo Market;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BetInfo
    {
        public long Id;
        public long AccountId;
        public long EventId;
        public long MarketId;
        public long SelectionId;
        public SideType Side;
        public decimal Price;
        public decimal Stake;
        public DateTime Created;
    }

    public enum SideType
    {
        Back,
        Lay
    }

    public struct MarketInfo
    {
        public long Id;
        public long EventId;
        
    }

    public enum EventType
    {
        Default
    }
}
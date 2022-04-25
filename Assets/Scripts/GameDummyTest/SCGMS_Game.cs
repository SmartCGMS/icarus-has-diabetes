using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameDummyTest
{
    public class SCGMS_Game
    {
        [DllImport("game-wrapper", EntryPoint = "scgms_game_create")]
        private static extern IntPtr Create(UInt16 configClass, UInt16 configId, UInt32 steppingMs, IntPtr logFilePath);

        [DllImport("game-wrapper", EntryPoint = "scgms_game_replay_create")]
        private static extern IntPtr CreateReplay(IntPtr logFilePath);

        [DllImport("game-wrapper", EntryPoint = "scgms_game_step")]
        private static extern Int32 Step(IntPtr game, Guid[] inputSignalIds, double[] inputSignalLevels, double[] inputSignalTimes, UInt32 inputSignalCount, out double bg, out double ig, out double iob, out double cob);

        [DllImport("game-wrapper", EntryPoint = "scgms_game_replay_step")]
        private static extern Int32 ReplayStep(IntPtr game, out Guid signal_id, out double level, out double time);

        [DllImport("game-wrapper", EntryPoint = "scgms_game_terminate")]
        private static extern Int32 Terminate(IntPtr game);

        // blood glucose ID
        public static readonly Guid signal_BG = new Guid("F666F6C2-D7C0-43E8-8EE1-C8CAA8F860E5");
        // interstitial glucose ID
        public static readonly Guid signal_IG = new Guid("3034568D-F498-455B-AC6A-BCF301F69C9E");
        // insulin on board ID
        public static readonly Guid signal_IOB = new Guid("313A1C11-6BAC-46E2-8938-7353409F2FCD");
        // carbs on board ID
        public static readonly Guid signal_COB = new Guid("B74AA581-538C-4B30-B764-5BD0D97B0727");

        // requested insulin basal rate signal ID
        public static readonly Guid signal_Requested_Insulin_Basal_Rate = new Guid("B5897BBD-1E32-408A-A0D5-C5BFECF447D9");
        // requested insulin bolus signal ID
        public static readonly Guid signal_Requested_Insulin_Bolus = new Guid("09B16B4A-54C2-4C6A-948A-3DEF8533059B");
        // delivered insulin basal rate signal ID
        public static readonly Guid signal_Delivered_Insulin_Basal_Rate = new Guid("BF88A8CB-1290-4477-A2CF-BDD06DF628AB");
        // delivered insulin bolus signal ID
        public static readonly Guid signal_Delivered_Insulin_Bolus = new Guid("22D87566-AF1B-4CC7-8D11-C5E04E1E9C8A");
        // requested carbohydrates intake signal ID
        public static readonly Guid signal_Carb_Intake = new Guid("37AA6AC1-6984-4A06-92CC-A660110D0DC7");
        // requested rescue carbohydrates intake signal ID
        public static readonly Guid signal_Carb_Rescue = new Guid("F24920F7-3F7B-4000-B2D0-374F940E4898");
        // requested physical activity signal ID
        public static readonly Guid signal_Physical_Activity = new Guid("F4438E9A-DD52-45BD-83CE-5E93615E62BD");

        // container for stored signal
        public class StoredSignal
        {
            // ID of the signal
            public Guid SignalId;
            // actual signal level
            public double Level;
            // timestamp; rat time, but never used as a rat time, rather than just an ordering field
            public double When;
        }

        // instance created with Create call (scgms_game_create)
        private IntPtr GameInstance;

        // last step's blood glucose value; set during regular gameplay and replays
        public double BloodGlucose { get; private set; }
        // last step's interstitial glucose value; set during regular gameplay and replays
        public double InterstitialGlucose { get; private set; }
        // last step's insulin on board value; set during regular gameplay and replays
        public double InsulinOnBoard { get; private set; }
        // last step's carbs on board value; set during regular gameplay and replays
        public double CarbohydratesOnBoard { get; private set; }

        // last step's physical activity; set only during replays
        public double PhysicalActivity { get; private set; }

        // list of newly acquired boluses; set only during replays
        public List<double> Boluses { get; private set; } = new List<double>();

        // list of newly acquired basal insulin rate changes; set only during replays
        public List<double> Basals { get; private set; } = new List<double>();

        // list of dosed regular carbs; set only during replays
        public List<double> CarbsRegular { get; private set; } = new List<double>();

        // list of dosed rescue carbs; set only during replays
        public List<double> CarbsRescue { get; private set; } = new List<double>();


        // signal IDs to be dosed in next step
        private List<Guid> InputIds = new List<Guid>();
        // signal levels to be dosed in next step
        private List<double> InputLevels = new List<double>();
        // signal times to be used in next step
        private List<double> InputTimes = new List<double>();

        // is this run a replay?
        private bool IsReplay = false;
        // list of all replayed signals
        private List<StoredSignal> ReplayedSignals = new List<StoredSignal>();
        // last replay timestamp
        private double LastReplayTime = -1;

        /// <summary>
        /// Game constructor - for use in regular game setup
        /// </summary>
        /// <param name="configClass"></param>
        /// <param name="configId"></param>
        /// <param name="steppingMs"></param>
        /// <param name="logFilePath"></param>
        /// <exception cref="Exception"></exception>
        public SCGMS_Game(UInt16 configClass, UInt16 configId, UInt32 steppingMs, String logFilePath)
        {
            IsReplay = false;
            IntPtr stringPtr = Marshal.StringToHGlobalAnsi(logFilePath);
            GameInstance = Create(configClass, configId, steppingMs, stringPtr);
            Marshal.FreeHGlobal(stringPtr);

            if (GameInstance == IntPtr.Zero)
               throw new Exception("Could not create game instance");
        }
        
        /// <summary>
        /// Replay constructor
        /// </summary>
        /// <param name="logFilePath">log file to be replayed</param>
        public SCGMS_Game(String logFilePath)
        {
            IsReplay = true;
            IntPtr stringPtr = Marshal.StringToHGlobalAnsi(logFilePath);
            GameInstance = CreateReplay(stringPtr);
            Marshal.FreeHGlobal(stringPtr);

            if (GameInstance == IntPtr.Zero)
                throw new Exception("Could not create game instance");

            ReplayedSignals = new List<StoredSignal>();

            // store all signals from gameplay, this class will then step through them without the wrapper's assistance
            Int32 res;
            do
            {
                Guid id;
                double level, time;

                res = ReplayStep(GameInstance, out id, out level, out time);

                if (res > 0)
                    ReplayedSignals.Add(new StoredSignal { SignalId = id, Level = level, When = time });

                LastReplayTime = -1;
            }
            while (res > 0);

            ReplayedSignals.Sort((x, y) => x.When.CompareTo(y.When));
        }

        /// <summary>
        /// Schedules a level event to be sent to a SCGMS backend
        /// </summary>
        /// <param name="id">signal ID</param>
        /// <param name="level">signal level</param>
        /// <param name="time">signal time (relative factor of step size, <0;1) )</param>
        public void ScheduleSignalLevel(Guid id, double level, double time)
        {
            InputIds.Add(id);
            InputLevels.Add(level);
            InputTimes.Add(time);
        }

        /// <summary>
        /// Schedules insulin bolus to be requested to a SCGMS backend
        /// </summary>
        /// <param name="level">amount to be delivered [U]</param>
        /// <param name="time">time to deliver (relative factor of step size, <0;1) )</param>
        public void ScheduleInsulinBolus(double level, double time)
        {
            ScheduleSignalLevel(signal_Requested_Insulin_Bolus, level, time);
        }

        /// <summary>
        /// Schedules insulin basal rate to be requested to a SCGMS backend
        /// </summary>
        /// <param name="level">amount to be requested [U/hr]</param>
        /// <param name="time">time to deliver (relative factor of step size, <0;1) )</param>
        public void ScheduleInsulinBasalRate(double level, double time)
        {
            ScheduleSignalLevel(signal_Requested_Insulin_Basal_Rate, level, time);
        }

        /// <summary>
        /// Schedules regular carbohydrates (CHO, meal) intake to be requested to a SCGMS backend
        /// </summary>
        /// <param name="level">amount to be delivered [g]</param>
        /// <param name="time">time to deliver (relative factor of step size, <0;1) )</param>
        public void ScheduleCarbohydratesIntake(double level, double time)
        {
            ScheduleSignalLevel(signal_Carb_Intake, level, time);
        }

        /// <summary>
        /// Schedules rescue carbohydrates (CHO) intake to be requested to a SCGMS backend
        /// </summary>
        /// <param name="level">amount to be delivered [g]</param>
        /// <param name="time">time to deliver (relative factor of step size, <0;1) )</param>
        public void ScheduleCarbohydratesRescue(double level, double time)
        {
            ScheduleSignalLevel(signal_Carb_Rescue, level, time);
        }

        /// <summary>
        /// Schedules physical activity to be sent to a SCGMS backend
        /// NOTE: there's no timer to cancel the excercise; when you want to end previously requested excercise, call this method with level = 0.0
        /// </summary>
        /// <param name="level">physical activity intensity [] (common values: 0.1 for light, 0.25 for medium and 0.4 for intensive excercise)</param>
        /// <param name="time">time to deliver (relative factor of step size, <0;1) )</param>
        public void SchedulePhysicalActivity(double level, double time)
        {
            ScheduleSignalLevel(signal_Physical_Activity, level, time);
        }

        /// <summary>
        /// Performs step in SCGMS game backend instance
        /// </summary>
        /// <returns>did the step succeed?</returns>
        public bool Step()
        {
            // cannot step on an invalid instance
            if (GameInstance == IntPtr.Zero)
                return false;

            // regular gameplay = step the wrapper (and the simulation) and extract only the needed signals
            if (!IsReplay)
            {
                double bg, ig, iob, cob;

                var res = Step(GameInstance, InputIds.ToArray(), InputLevels.ToArray(), InputTimes.ToArray(), (UInt32)InputIds.Count, out bg, out ig, out iob, out cob);

                InputIds.Clear();
                InputLevels.Clear();
                InputTimes.Clear();

                BloodGlucose = bg;
                InterstitialGlucose = ig;
                InsulinOnBoard = iob;
                CarbohydratesOnBoard = cob;

                return (res > 0);
            }
            else // a replay = step just internally, the simulation/wrapper just waits to be terminated properly
            {
                if (ReplayedSignals.Count == 0)
                    return false;

                CarbsRegular.Clear();
                CarbsRescue.Clear();
                Boluses.Clear();
                Basals.Clear();

                // one step = take out all signals with the latest timestamp and sort them to properties
                LastReplayTime = ReplayedSignals[0].When;
                while (ReplayedSignals.Count > 0 && ReplayedSignals[0].When == LastReplayTime)
                {
                    if (ReplayedSignals[0].SignalId == signal_Carb_Intake)
                        CarbsRegular.Add(ReplayedSignals[0].Level);
                    else if (ReplayedSignals[0].SignalId == signal_Carb_Rescue)
                        CarbsRescue.Add(ReplayedSignals[0].Level);
                    else if (ReplayedSignals[0].SignalId == signal_Delivered_Insulin_Bolus || ReplayedSignals[0].SignalId == signal_Requested_Insulin_Bolus)
                        Boluses.Add(ReplayedSignals[0].Level);
                    else if (ReplayedSignals[0].SignalId == signal_Delivered_Insulin_Basal_Rate || ReplayedSignals[0].SignalId == signal_Requested_Insulin_Basal_Rate)
                        Basals.Add(ReplayedSignals[0].Level);
                    else if (ReplayedSignals[0].SignalId == signal_BG)
                        BloodGlucose = ReplayedSignals[0].Level;
                    else if (ReplayedSignals[0].SignalId == signal_IG)
                        InterstitialGlucose = ReplayedSignals[0].Level;
                    else if (ReplayedSignals[0].SignalId == signal_IOB)
                        InsulinOnBoard = ReplayedSignals[0].Level;
                    else if (ReplayedSignals[0].SignalId == signal_COB)
                        CarbohydratesOnBoard = ReplayedSignals[0].Level;
                    else if (ReplayedSignals[0].SignalId == signal_Physical_Activity)
                        PhysicalActivity = ReplayedSignals[0].Level;

                    // pop the record
                    ReplayedSignals.RemoveAt(0);
                }

                return true;
            }
        }

        /// <summary>
        /// Terminates SCGMS backend execution; this method may block
        /// </summary>
        /// <returns>did the termination succeed?</returns>
        public bool Terminate()
        {
            // cannot terminate an empty instance
            if (GameInstance == IntPtr.Zero)
                return false;

            bool result = (Terminate(GameInstance) > 0);

            // clear the instance
            if (result)
                GameInstance = IntPtr.Zero;

            return result;
        }
    }
}

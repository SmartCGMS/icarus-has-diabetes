using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameDummyTest
{
    public class SCGMS_Game_Opt
    {
        public enum Optimizer_Status
        {
            None = 0,       // optimalization hasn't started yet; this should never be exported through library interface, as this state gets immediatelly replaced by either Running of Failed state
            Running = 1,    // in progress
            Success = 2,    // finished successfully
            Failed = 3,
        };

        [DllImport("game-wrapper", EntryPoint = "scgms_game_optimize")]
        private static extern IntPtr Optimize(UInt16 configClass, UInt16 configId, UInt32 steppingMs, IntPtr logFileInPath, IntPtr logFileOutPath, UInt16 degreeOfOpt);

        [DllImport("game-wrapper", EntryPoint = "scgms_game_get_optimize_status")]
        private static extern Int32 Get_Optimize_Status(IntPtr gameopt, out UInt32 status, out double progress_pct);

        [DllImport("game-wrapper", EntryPoint = "scgms_game_cancel_optimize")]
        private static extern Int32 Cancel_Optimize(IntPtr gameopt, Int32 wait_for_cancel);
		
		[DllImport("game-wrapper", EntryPoint = "scgms_game_optimizer_terminate")]
        private static extern Int32 Terminate(IntPtr gameopt);

        // internal instance of game optimizer; used only as a first ("thiscall") parameter of external function calls
        private IntPtr GameOptInstance;

        /// <summary>
        /// Constructor, creating optimalization context and immediatelly starting optimalization
        /// </summary>
        /// <param name="configClass">config class used</param>
        /// <param name="configId">config ID within class</param>
        /// <param name="steppingMs">stepping; should be the same as the one in regular game wrapper</param>
        /// <param name="logFileInPath">input log file (logged during the regular game)</param>
        /// <param name="logFileOutPath">output log file (to be replayed as "optimal")</param>
        /// <param name="degreeOfOpt">how thoroughly to optimalize (0 = not at all, 100 = 100% of default iterations)</param>
        /// <exception cref="Exception"></exception>
        public SCGMS_Game_Opt(UInt16 configClass, UInt16 configId, UInt32 steppingMs, String logFileInPath, String logFileOutPath, UInt16 degreeOfOpt)
        {
            IntPtr stringInPtr = Marshal.StringToHGlobalAnsi(logFileInPath);
            IntPtr stringOutPtr = Marshal.StringToHGlobalAnsi(logFileOutPath);
            GameOptInstance = Optimize(configClass, configId, steppingMs, stringInPtr, stringOutPtr, degreeOfOpt);
            Marshal.FreeHGlobal(stringInPtr);
            Marshal.FreeHGlobal(stringOutPtr);

            if (GameOptInstance == IntPtr.Zero)
                throw new Exception("Could not create game optimizer instance");
        }

        /// <summary>
        /// Retrieves the optimalization status
        /// </summary>
        /// <param name="pct">percentage of progress, values from 0 to 1</param>
        /// <returns>current optimalization status</returns>
        /// <exception cref="Exception"></exception>
        public Optimizer_Status Get_Status(out double pct)
        {
            UInt32 status;

            if (Get_Optimize_Status(GameOptInstance, out status, out pct) == 0)
                throw new Exception("Could not retrieve optimizer status");

            return (Optimizer_Status)status;
        }

        /// <summary>
        /// Terminates the optimalization, if there is any in progress
        /// </summary>
        /// <param name="wait_for_cancel">whould we wait for the optimalization to be cancelled?</param>
        /// <returns>success indicator</returns>
		public bool Cancel_Optimalization(bool wait_for_cancel)
		{
			var status = Cancel_Optimize(GameOptInstance, wait_for_cancel ? 1 : 0);

            return status != 0;
		}

        /// <summary>
        /// When the optimalization ends, this method should be called in order to finalize outputs and terminate the wrapper
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Finalize_Output()
        {
            var result = Terminate(GameOptInstance);

            if (result == 0)
                throw new Exception("Could not terminate optimizer correctly");
        }
    }
}

// a type for interop-exportable pointer to CGame_Wrapper instance; the pointer should never be dereferenced in outer code as the CGame_Wrapper class is not designed to be interoperable
using scgms_game_wrapper_t = CGame_Wrapper*;

/*
 * scgms_game_create
 *
 * Creates game wrapper instance with given parameters.
 *
 * Parameters:
 *		config_class - category of configs to be used; this is more like an attept to split difficulties and patient types
 *		config_id - config identifier within selected config class
 *		stepping_ms - model stepping in milliseconds - subsequent scgms_game_step calls would step the model by this exact amount of milliseconds
 *		log_file_path - where to put the log file; empty or nullptr to indicate the intent to discard the log
 *
 * Return values:
 *		<valid scgms_game_wrapper_t> - success
 *		nullptr - failure
 */
extern "C" scgms_game_wrapper_t IfaceCalling scgms_game_create(uint16_t config_class, uint16_t config_id, uint32_t stepping_ms, const char* log_file_path);

/*
 * scgms_game_step
 *
 * Performs a single step within the simulation, injects all necessary events according to given parameters
 *
 * Parameters:
 *		wrapper - pointer to a game wrapper instance obtained from scgms_game_create call
 *		input_signal_ids - array of signal GUIDs
 *		input_signal_levels - array of levels
 *		input_signal_times - array of times; times are a relative factor of step, range <0;1)
 *		input_signal_count - count of input signal arrays (input_signal_ids, input_signal_levels, input_signal_times)
 *		bg - output variable for blood glucose reading [mmol/L]
 *		ig - output variable for interstitial glucose reading [mmol/L]
 *		iob - output variable for current model insulin on board [U]
 *		cob - output variable for current model carbohydrates on board [g]
 *
 * Return values:
 *		TRUE (non-zero) - success
 *		FALSE (zero) - failure - parameters are invalid or the attempt to step the model has failed
 */
extern "C" BOOL IfaceCalling scgms_game_step(scgms_game_wrapper_t wrapper, GUID* input_signal_ids, double* input_signal_levels, double* input_signal_times, uint32_t input_signal_count, double* bg, double* ig, double* iob, double* cob);

/*
 * scgms_game_terminate
 *
 * Terminates the running game, invalidates the obtained scgms_game_wrapper_t object. May block due to yet unprocessed events.
 *
 * Parameters:
 *		wrapper - pointer to a game wrapper instance obtained from scgms_game_create call
 *
 * Return values:
 *		TRUE (non-zero) - success
 *		FALSE (zero) - failure
 */
extern "C" BOOL IfaceCalling scgms_game_terminate(scgms_game_wrapper_t wrapper);
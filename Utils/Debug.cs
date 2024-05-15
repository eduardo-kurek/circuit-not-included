namespace mod_oni
{
	public static class Debug
	{
		public static string ModAbbr = "CNI";

		public static void LogWarning(object msg){
			global::Debug.LogWarning($"[{ModAbbr}] {msg}");
		}

		public static void Log(object msg){
			global::Debug.Log($"[{ModAbbr}] {msg}");
		}
		
		public static void LogError(object msg){
			global::Debug.LogError($"[{ModAbbr}] {msg}");
		}
		
	}
}
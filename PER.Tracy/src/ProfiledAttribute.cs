using Metalama.Framework.Aspects;

namespace PER.Tracy;

[RequireAspectWeaver("PER.Tracy.Weaver.ProfiledWeaver")]
public class ProfiledAttribute : TypeAspect { }

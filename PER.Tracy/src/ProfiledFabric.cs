using JetBrains.Annotations;

using Metalama.Framework.Fabrics;

namespace PER.Tracy;

[UsedImplicitly]
public class ProfiledFabric : TransitiveProjectFabric {
    public override void AmendProject(IProjectAmender amender) =>
        amender.Outbound.SelectMany(project => project.Types).AddAspect<ProfiledAttribute>();
}

using Metalama.Framework.Fabrics;

namespace PER.Tracy.Weaver;

// ReSharper disable once UnusedType.Global
public class ProfiledFabric : TransitiveProjectFabric {
    public override void AmendProject(IProjectAmender amender) =>
        amender.With(project => project.Types).AddAspect<ProfiledAttribute>();
}

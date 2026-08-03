using Fallout.Common;

namespace Automation.Fallout.Components.Parameters;

public interface IHasCodeSigning : IFalloutBuild
{
    [Parameter] string SslComUsername => TryGetValue(() => SslComUsername) ?? "llanphear@promiles.com";
    [Parameter] string SslComPassword => TryGetValue(() => SslComPassword) ?? "DzA4$gkgc3M8?#RG";
    [Parameter] string SslComCredentialId => TryGetValue(() => SslComCredentialId) ?? " a60-1khrrv6";
    [Parameter] string CodeSigningCertificateName => TryGetValue(() => CodeSigningCertificateName) ?? "Promiles Software Development";
    
}
// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "Need to add custom exception")]
[assembly: SuppressMessage("Major Code Smell", "S112:General exceptions should never be thrown", Justification = "Need to add custom exception")]
[assembly: SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used", Justification = "Could not load assembly with Load(Error), Hence using LoadFrom")]

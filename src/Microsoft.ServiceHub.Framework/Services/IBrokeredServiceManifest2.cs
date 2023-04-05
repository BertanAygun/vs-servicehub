// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.ServiceHub.Framework.Services;

#pragma warning disable RS0016 // Add public types and members to the declared API

/// <summary>
/// Exposes details about availability of services proffered to the client and their registration.
/// Obtainable from the <see cref="FrameworkServices.RemoteBrokeredServiceManifest"/> service.
/// </summary>
/// <remarks>
/// The results are based on the caller.
/// For example if an instance of this service is obtained by a Live Share guest
/// the results from method calls may vary from an instance of this service obtained by a Codespaces client.
/// </remarks>
public interface IBrokeredServiceManifest2
{
	/// <summary>
	/// Raised when services registrations are updated for registered contract types.
	/// </summary>
	event EventHandler<ManifestUpdatedEventArgs>? ServiceRegistrationsUpdated;

	/// <summary>
	/// Subscribes to receive service registration change notifications for either specified contract or all services.
	/// </summary>
	/// <param name="contractName">Optional contract name to subscribe to, if null subscription will apply to all services.</param>
	/// <param name="cancellationToken">Cancellation token to monitor.</param>
	/// <returns>an IDisposable that can be disposed when subscription is terminated.</returns>
	/// <remarks>An initial ServiceRegistrationsUpdated event will always be sent following this call with all the existing services,
	/// so please ensure an event handler is registered before calling this method.</remarks>
	Task<IDisposable> SubscribeForServicesAsync(string? contractName, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the list of services that are available from an <see cref="IServiceBroker"/>.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A collection of service registrations.</returns>
	ValueTask<IReadOnlyCollection<BrokeredServiceRegistration>> GetAvailableServicesAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Gets the collection of registrations available for the specified service from an <see cref="IServiceBroker"/>.
	/// </summary>
	/// <param name="serviceName">The <see cref="ServiceMoniker.Name"/> from the <see cref="ServiceMoniker"/> for the service to get information about.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// A collection of service registrations available for the named service.
	/// </returns>
	ValueTask<ImmutableSortedSet<BrokeredServiceRegistration?>> GetAvailableVersionsAsync(string serviceName, CancellationToken cancellationToken);
}

/// <summary>
/// A service registration encapsulating moniker, contract and metadata.
/// </summary>
/// <param name="ServiceMoniker">Service moniker that registration belongs to.</param>
/// <param name="Contract">Optional contract name that this service moniker implements.</param>
/// <param name="Metadata">Optional metadata that belongs to service moniker and contract pair.</param>
/// <remarks>
/// Making registration specific to a contract and metadata pair provides most flexibility for future cases.
/// In cases where a service implements multiple contracts, we would have multiple registration entries.
/// </remarks>
public record BrokeredServiceRegistration(
	ServiceMoniker ServiceMoniker,
	string? Contract,
	IReadOnlyDictionary<string, string>? Metadata);

/// <summary>
/// Describes changes to brokered service registrations.
/// </summary>
/// <remarks>
/// If the same service moniker is updated with new metadata, the removal and addition is expected to be on the same event arguments if possible.
/// </remarks>
public class ManifestUpdatedEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ManifestUpdatedEventArgs"/> class with new and removed services.
	/// </summary>
	/// <param name="addedServices">The set of services that are added by this change.</param>
	/// <param name="removedServices">The set of service registrations that are removed by this change.</param>
	public ManifestUpdatedEventArgs(
		IImmutableSet<BrokeredServiceRegistration> addedServices,
		IImmutableSet<BrokeredServiceRegistration> removedServices)
	{
		// Do we need to optimize this further to allow nulls or can we just rely on empty lists?
	}

	/// <summary>
	/// Gets the set of service registrations that are added since the last update.
	/// </summary>
	public IImmutableSet<BrokeredServiceRegistration> AddedServices { get; }

	/// <summary>
	/// Gets the set of service registrations that were removed since the last update.
	/// </summary>
	public IImmutableSet<BrokeredServiceRegistration> RemovedServices { get; }
}

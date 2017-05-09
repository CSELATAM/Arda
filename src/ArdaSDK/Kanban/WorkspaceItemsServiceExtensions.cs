// Code generated by Microsoft (R) AutoRest Code Generator 1.0.1.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace ArdaSDK.Kanban
{
    using Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for WorkspaceItemsService.
    /// </summary>
    public static partial class WorkspaceItemsServiceExtensions
    {
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='itemId'>
            /// </param>
            public static WorkspaceItem GetItem(this IWorkspaceItemsService operations, System.Guid itemId)
            {
                return operations.GetItemAsync(itemId).GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='itemId'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<WorkspaceItem> GetItemAsync(this IWorkspaceItemsService operations, System.Guid itemId, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetItemWithHttpMessagesAsync(itemId, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='itemId'>
            /// </param>
            /// <param name='newItem'>
            /// </param>
            public static void Edit(this IWorkspaceItemsService operations, System.Guid itemId, WorkspaceItem newItem = default(WorkspaceItem))
            {
                operations.EditAsync(itemId, newItem).GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='itemId'>
            /// </param>
            /// <param name='newItem'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task EditAsync(this IWorkspaceItemsService operations, System.Guid itemId, WorkspaceItem newItem = default(WorkspaceItem), CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.EditWithHttpMessagesAsync(itemId, newItem, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='itemId'>
            /// </param>
            public static void Delete(this IWorkspaceItemsService operations, System.Guid itemId)
            {
                operations.DeleteAsync(itemId).GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='itemId'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task DeleteAsync(this IWorkspaceItemsService operations, System.Guid itemId, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.DeleteWithHttpMessagesAsync(itemId, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='itemId'>
            /// </param>
            /// <param name='newStatus'>
            /// </param>
            public static void UpdateStatus(this IWorkspaceItemsService operations, System.Guid itemId, int newStatus)
            {
                operations.UpdateStatusAsync(itemId, newStatus).GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='itemId'>
            /// </param>
            /// <param name='newStatus'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task UpdateStatusAsync(this IWorkspaceItemsService operations, System.Guid itemId, int newStatus, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.UpdateStatusWithHttpMessagesAsync(itemId, newStatus, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

    }
}
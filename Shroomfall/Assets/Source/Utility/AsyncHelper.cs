using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Service;
using System;
using System.Threading.Tasks;

/*
 * Presenter / Application layer helper.
 *
 * Purpose:
 * - Safely run fire-and-forget async workflows from Unity callbacks.
 * - Catch both sync and async exceptions. (MAIN REASON)
 * - Report errors on the Unity main thread. (MAIN REASON)
 *
 * NOT intended for:
 * - Service layer
 * - Domain logic
 * - State mutation
 */

namespace Assets.Source.Utility
{
    public static class AsyncHelper
    {
        #region Attributes
        #endregion

        #region Properties
        #endregion

        #region Methods
        public static void Run(
            UIService uiService,
            Func<Task> taskFactory)
        {
            Run(async () =>
            {
                CoroutineRunner.Instance.Schedule(() =>
                {
                    uiService.ShowLoading(true);
                    uiService.SetGlobalInteractable(false);
                });

                try
                {
                    await taskFactory();
                }
                catch (Exception ex)
                {
                    CoroutineRunner.Instance.Schedule(() =>
                    {
                        uiService.ShowToast(ToastType.Error, ex.Message);
                    });

                    throw;
                }
                finally
                {
                    CoroutineRunner.Instance.Schedule(() =>
                    {
                        uiService.ShowLoading(false);
                        uiService.SetGlobalInteractable(true);
                    });
                }
            });
        }

        public static void Run(
            Func<Task> taskFactory)
        {
            Task task;

            try
            {
                task = taskFactory();
            }
            catch (Exception ex)
            {
                return;
            }
        }
        #endregion
    }
}
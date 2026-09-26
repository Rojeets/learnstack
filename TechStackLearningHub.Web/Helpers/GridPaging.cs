using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TechStackLearningHub.Web.Helpers
{
    /// <summary>
    /// Paging support shared by the grids that list a whole table.
    ///
    /// GridView renders the pager links but does not carry the requested index
    /// through to the rows it renders: the postback reaches the grid and raises
    /// PageIndexChanging with the right index, yet page 1 is still rendered. So
    /// the index is read off the postback and applied by the page before it
    /// binds.
    ///
    /// The count has to come from current data too. A GridView restores its row
    /// count from view state, so a tab left open on page 3 can post back that
    /// index after the rows behind it were deleted or filtered away, and asking
    /// for a page that no longer exists faults the request instead of showing a
    /// list.
    ///
    /// One rule matters more than the rest: a page must not re-bind its grid on
    /// an ordinary postback. The row action links register themselves for event
    /// validation as they render, and re-binding rebuilds the row controls, so
    /// the incoming target stops validating and every row action fails with
    /// "Invalid postback or callback argument". Bind on the first request and on
    /// a pager postback, and leave row actions on the path they already had.
    /// </summary>
    public static class GridPaging
    {
        private const string PagePrefix = "Page$";

        /// <summary>
        /// True when this postback is a pager click on <paramref name="grid"/>,
        /// which is the one postback a paged grid may re-bind for.
        /// </summary>
        public static bool IsPagerRequest(Page page, GridView grid)
        {
            if (page == null || grid == null || !page.IsPostBack)
                return false;

            string target = page.Request.Form["__EVENTTARGET"];
            if (string.IsNullOrEmpty(target))
                return false;

            if (!string.Equals(target, grid.UniqueID, StringComparison.OrdinalIgnoreCase))
                return false;

            string argument = page.Request.Form["__EVENTARGUMENT"];
            return !string.IsNullOrEmpty(argument)
                && argument.StartsWith(PagePrefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The zero-based page the reader asked for, or null if this is not a
        /// pager click on the grid. Call after binding, then hand the result to
        /// <see cref="ApplyIndex"/>.
        /// </summary>
        public static int? RequestedPage(Page page)
        {
            string argument = page.Request.Form["__EVENTARGUMENT"];
            if (string.IsNullOrEmpty(argument))
                return null;

            if (!argument.StartsWith(PagePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            int requested;
            if (!int.TryParse(argument.Substring(PagePrefix.Length), out requested))
                return null;

            return requested - 1; // the rendered pager is 1-based
        }

        /// <summary>
        /// Moves the grid to the requested page, or to the first page when the
        /// current data no longer has that page.
        /// </summary>
        public static void ApplyIndex(GridView grid, int requested)
        {
            grid.PageIndex = requested >= 0 && requested <= grid.PageCount - 1 ? requested : 0;
        }

        /// <summary>
        /// Refuses a page change the current data cannot satisfy, as a backstop
        /// for the index this class applies. The grid must already be bound when
        /// this runs, because the check reads its page count.
        /// </summary>
        public static void Wire(GridView grid)
        {
            if (grid == null)
                return;

            grid.PageIndexChanging += (sender, e) =>
            {
                if (e.NewPageIndex < 0 || e.NewPageIndex > grid.PageCount - 1)
                    e.Cancel = true;
            };
        }

        public static void Rebind(GridView grid)
        {
            grid.DataBind();
            Clamp(grid);
        }

        /// <summary>
        /// Steps back to the last page that still has rows. Used after a delete,
        /// where the grid is holding the index of the page that just went away.
        /// </summary>
        public static void Clamp(GridView grid)
        {
            if (grid.PageIndex > grid.PageCount - 1)
                grid.PageIndex = Math.Max(0, grid.PageCount - 1);
        }
    }
}

<%@ Page Title="Lesson" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LessonViewer.aspx.cs" Inherits="TechStackLearningHub.Web.Student.LessonViewer" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlLesson" runat="server">
        <a class="btn btn-link ps-0" id="lnkBack" runat="server">&laquo; Back</a>
        <h1><asp:Literal ID="litLessonTitle" runat="server" /></h1>

        <div class="mb-4">
            <asp:Literal ID="litContentHtml" runat="server" />
        </div>

        <asp:Panel ID="pnlVideo" runat="server" Visible="false" class="mb-4">
            <div class="ratio ratio-16x9">
                <iframe id="frmVideo" runat="server" title="Lesson video" allowfullscreen="true"></iframe>
            </div>
        </asp:Panel>

        <asp:HyperLink ID="hlDownloadNotes" runat="server" CssClass="btn btn-outline-secondary mb-3" Visible="false" Text="Download notes" />

        <div class="mt-4">
            <asp:Button ID="btnMarkComplete" runat="server" Text="Mark as Complete" CssClass="btn btn-success"
                OnClick="btnMarkComplete_Click" OnClientClick="this.disabled=true;this.value='Marked Completed';" />
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <div class="alert alert-warning">This lesson is not available.</div>
    </asp:Panel>
</asp:Content>
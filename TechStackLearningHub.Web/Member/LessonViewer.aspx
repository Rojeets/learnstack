<%@ Page Title="Lesson" Language="C#" MasterPageFile="~/Masterpages/Site.Master" AutoEventWireup="true" CodeBehind="LessonViewer.aspx.cs" Inherits="TechStackLearningHub.Web.Member.LessonViewer" %>

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

        <div class="mt-4" id="completionArea">
            <asp:Panel ID="pnlCompleted" runat="server" CssClass="alert alert-success d-none" role="status">
                <span class="fw-semibold">Lesson complete.</span>
                Nice work - your progress has been saved.
            </asp:Panel>
            <asp:Button ID="btnMarkComplete" runat="server" Text="Mark as complete" CssClass="btn btn-success"
                OnClick="btnMarkComplete_Click" OnClientClick="this.disabled=true;" />
        </div>

        <nav class="mt-4 d-flex justify-content-between gap-2" aria-label="Lesson navigation">
            <asp:HyperLink ID="lnkPrevLesson" runat="server" Visible="false" CssClass="btn btn-outline-secondary" />
            <asp:HyperLink ID="lnkNextLesson" runat="server" Visible="false" CssClass="btn btn-outline-primary ms-auto" />
        </nav>
    </asp:Panel>

    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <div class="alert alert-warning">This lesson is not available.</div>
    </asp:Panel>
</asp:Content>
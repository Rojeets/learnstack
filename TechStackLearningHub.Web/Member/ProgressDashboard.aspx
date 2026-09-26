<%@ Page Title="Progress" Language="C#" MasterPageFile="~/Masterpages/Site.Master" AutoEventWireup="true" CodeBehind="ProgressDashboard.aspx.cs" Inherits="TechStackLearningHub.Web.Member.ProgressDashboard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>My Progress</h1>
    <p class="lead">Course completion across every published course you have started.</p>

    <asp:Repeater ID="rptCourses" runat="server">
        <ItemTemplate>
            <div class="card mb-3">
                <div class="card-body">
                    <div class="d-flex justify-content-between">
                        <h2 class="h5 card-title"><%# Eval("CourseName") %></h2>
                        <span class="badge bg-primary"><%# Eval("TechStack") %></span>
                    </div>
                    <div class="progress" role="progressbar" aria-valuenow="<%# Eval("PercentComplete") %>" aria-valuemin="0" aria-valuemax="100">
                        <div class="progress-bar" style='width: <%# Eval("PercentComplete") %>%'></div>
                    </div>
                    <small class="text-muted"><%# Eval("PercentComplete") %>% complete</small>
                </div>
            </div>
        </ItemTemplate>
        </asp:Repeater>
    <asp:Panel ID="pnlEmpty" runat="server" CssClass="alert alert-info" Visible="false">
        You have not completed any lessons yet. <a href="CourseCatalog.aspx">Start a course</a>.
    </asp:Panel>
</asp:Content>
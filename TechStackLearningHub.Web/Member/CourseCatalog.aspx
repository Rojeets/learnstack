<%@ Page Title="Course Catalogue" Language="C#" MasterPageFile="~/Masterpages/Site.Master" AutoEventWireup="true" CodeBehind="CourseCatalog.aspx.cs" Inherits="TechStackLearningHub.Web.Member.CourseCatalog" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Course Catalogue</h1>
    <div class="row mb-4">
        <div class="col-md-4">
            <asp:Label runat="server" AssociatedControlID="ddlTechStackFilter" CssClass="form-label" Text="Filter by tech stack" />
            <asp:DropDownList ID="ddlTechStackFilter" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlTechStackFilter_SelectedIndexChanged" />
        </div>
    </div>

    <asp:Repeater ID="rptCourses" runat="server">
        <ItemTemplate>
            <div class="card mb-3">
                <div class="card-body">
                    <div class="d-flex justify-content-between">
                        <h2 class="h5 card-title"><%# Eval("CourseName") %></h2>
                        <span class="badge bg-primary"><%# Eval("TechStack") %></span>
                    </div>
                    <p class="card-text"><%# Eval("Description") %></p>
                    <p class="card-text small text-muted"><%# Eval("ModuleCount") %> modules</p>
                    <a class="btn btn-outline-primary" href='CourseDetails.aspx?CourseID=<%# Eval("CourseID") %>'>View course</a>
                </div>
            </div>
        </ItemTemplate>
        </asp:Repeater>
    <asp:Panel ID="pnlEmpty" runat="server" CssClass="alert alert-info" Visible="false">
        No published courses for this stack yet.
    </asp:Panel>
</asp:Content>
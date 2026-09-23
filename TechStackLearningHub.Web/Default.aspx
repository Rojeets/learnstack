<%@ Page Title="TechStack Learning Hub" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="TechStackLearningHub.Web._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row">
        <div class="col-lg-8 mx-auto text-center">
            <h1 class="display-5 fw-bold">Learn your stack.</h1>
            <p class="lead">A role-based e-learning hub for ASP.NET, React, Python, Java, Node.js, PHP/Laravel, Android (Kotlin) and Flutter.</p>
            <div class="d-grid gap-2 d-sm-flex justify-content-sm-center">
                <a class="btn btn-primary btn-lg px-4" href="~/Register.aspx">Create an account</a>
                <a class="btn btn-outline-secondary btn-lg px-4" href="~/Login.aspx">Log in</a>
            </div>
        </div>
    </div>

    <div class="row mt-5 g-4">
        <div class="col-md-4">
            <div class="card h-100">
                <div class="card-body">
                    <h5 class="card-title">Structured courses</h5>
                    <p class="card-text">Courses are split into ordered modules and lessons with embedded video and downloadable notes.</p>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card h-100">
                <div class="card-body">
                    <h5 class="card-title">Quizzes with pass marks</h5>
                    <p class="card-text">Each module ends with a weighted quiz. Scores count towards your progress dashboard and history.</p>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card h-100">
                <div class="card-body">
                    <h5 class="card-title">Progress you can see</h5>
                    <p class="card-text">Completion percentages update as you mark lessons complete and pass quizzes.</p>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
$(document).ready(function() {
    $("#domainRequiredBox").change(function () {
        $("#groupDomainDiv").toggle();
    });
    
    $("#btnCreateGroup").click(submitGroup);
    
    function submitGroup() {
        console.log("submit");
        let group = {
            name: $("#groupName").val(),
            region: $("#groupRegion").val(),
            isPublic: $("#isPublicBox").prop("checked"),
            requiresDomain: $("#domainRequiredBox").prop("checked")
        };
        console.log(group);
        axios.post("/api/group/create", JSON.stringify(group), global.header).then(function(response) {
            if (global.debug) util.logResponse(response);
            
            if(response.status === 200)
                window.location.href = "/Group/" + response.data;
            
        }).catch(function(e) {
            util.logResponse(e);
        });
    }
});
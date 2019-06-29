var image_index = 1;
var ver = 1;

$(document).ready(function () {

    $("#loading").css("left", (window.innerWidth - 400) / 2);
    $("#loading").css("top", (window.innerHeight - 300) / 2);
    $("#loading").hide();

    populateInceptionTrainImages();

    $("#objects").on("click", "li", function () {
        $("#test_name").val($(this).text());
        $("#test_image").attr("src", "images1/" + $(this).text() + ".jpg");
        $("#test_status").empty();
    });    

    $("#train").click(function () {
        startTraining();
    });

    //$("#capture_train_image").click(function () {
    //    startCapturingTrainImage();
    //});    

    $("#capture_test_image").click(function () {
        startCapturingTestImage();
    });    

    $("#predict_test_image").click(function () {
        startPredictingTestImage();
    });

    $("#score_test_image").click(function () {
        startScoringTestImage();
    });

    //$("#predict_images").click(function () {
    //    startPredictingImages();
    //});

    //$("#classify_test_image").click(function () {
    //    startClassifyingTestImage();
    //});
});

function startTraining() {

    $("#loading").show();
    $.get("/api/images/train", function (data, status) {
        
        if (status === "success") {
            $("#train_status").html("Training completed"); 
        }
        else {
            $("#train_status").html("Training failure");                    
        }
        $("#loading").hide();
    });            
}

function startCapturingTrainImage() {

    var image_name = $("#train_name").val();

    $("#loading").show();
    $.get(`/api/images/down/${image_name}/${image_index}`, function (data, status) {
        if (status === "success") {
            $("#train_images").append(`<img src="/images/${image_name}${image_index}.jpg" width="160" alt="${image_name}${image_index}.jpg" /><span>${image_name}${image_index}</span>`);
            image_index++;

            populateObjects();
        }
        else {
            $("#train_status").html("Downloading failure");               
        }
        $("#loading").hide();
    });    
}

function startCapturingTestImage() {

    var image_name = $("#test_name").val();

    $("#loading").show();
    $.get(`/api/images/down/${image_name}`, function (data, status) {
        if (status === "success") {
            $("#test_images").html(`<img src="/images/${image_name}.jpg?ver=${ver++}" class="img-thumbnail" width="160" alt="${image_name}.jpg" /><span>${image_name}</span>`);
        }
        else {
            $("#test_status").html("Downloading failure");
        }
        $("#loading").hide();
    });
}

//function startPredictingImages() {
//    $("#loading").show();
//    $.get("/api/images/predict/", function (data, status) {

//        if (status === "success") {
//            $("#test_status").html(data);
//        }
//        else {
//            $("#test_status").html("Testing failure");
//        }
//        $("#loading").hide();
//    });
//}

function startPredictingTestImage() {
    var imageName = $("#test_name").val();

    $("#loading").show();
    $.get("/api/images/predict/" + imageName + ".jpg", function (data, status) {

        if (status === "success") {
            $("#test_status").html(data);
        }
        else {
            $("#test_status").html("Testing failure");
        }
        $("#loading").hide();
    });
}

function startScoringTestImage() {
    var imageName = $("#test_name").val();

    $("#loading").show();
    $.get("/api/images/score/" + imageName + ".jpg", function (data, status) {

        if (status === "success") {
            $("#test_status").html(data);
        }
        else {
            $("#test_status").html("Testing failure");
        }
        $("#loading").hide();
    });
}

//function startClassifyingTestImage() {
//    var imageName = $("#test_name").val();

//    $("#loading").show();
//    $.get("/api/images/classify/" + imageName + ".jpg", function (data, status) {

//        if (status === "success") {
//            $("#test_status").html(data);
//        }
//        else {
//            $("#test_status").html("Testing failure");
//        }
//        $("#loading").hide();
//    });
//}


function populateInceptionTrainImages() {
    $("#loading").show();
    $.get("/api/images/inception/train/image", function (data, status) {
        if (status === "success") {

            $("#objects").empty();

            for (var i = 0; i < data.length; i++) {
                $("#objects").append(`<li class="list-inline-item p-1 list-group-item-primary">${data[i]}</li>`);
            }
        }
        $("#loading").hide();
    });    
}
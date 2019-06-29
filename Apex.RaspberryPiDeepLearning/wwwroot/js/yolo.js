var image_index = 1;
var ver = 1;
var background;
var canvas;
var context;
var yolo_ratio_x;
var yolo_ratio_y;
var scale;
var ratio;

$(document).ready(function () {

    $("#loading").css("left", (window.innerWidth - 400) / 2);
    $("#loading").css("top", (window.innerHeight - 300) / 2);
    $("#loading").hide();

    populateYoloImages();

    //$("#train").click(function () {
    //    startTraining();
    //});

    //$("#capture_train_image").click(function () {
    //    startCapturingTrainImage();
    //}); 

    $("#objects").on("click", "li", function () {
        $("#test_name").val($(this).text());
        $("#test_image").attr("src", "images3/" + $(this).text() + ".jpg");
        $("#test_status").empty();
        $("canvas").remove();

        background = new Image();
        background.src = "images3/" + $("#test_name").val() + ".jpg";

        background.onload = function () {
            ratio = background.height / background.width;

            canvas = document.createElement("canvas");
            canvas.classList.add("img-fluid");
            canvas.classList.add("img-thumbnail");
            canvas.classList.add("m-2");
            canvas.width = $("#test_image_canvas").width();

            scale = canvas.width / background.width;
            canvas.height = background.height * scale;

            yolo_ratio_x = canvas.width / 416;
            yolo_ratio_y = canvas.height / 416;

            context = canvas.getContext("2d");
            context.drawImage(background, 0, 0, canvas.width, canvas.height);
            context.stroke();

            let container = document.getElementById("test_image_canvas");
            container.appendChild(canvas);
        };
    });    

    $("#capture_test_image").click(function () {
        startCapturingTestImage();
    });    

    $("#predict_test_image").click(function () {
        startPredictingTestImage();
    });  

    //$("#predict_images").click(function () {
    //    startPredictingImages();
    //});

    //$("#classify_test_image").click(function () {
    //    startClassifyingTestImage();
    //});
});

//function startTraining() {

//    $("#loading").show();
//    $.get("/api/images/train", function (data, status) {
        
//        if (status === "success") {
//            $("#train_status").html("Training completed"); 
//        }
//        else {
//            $("#train_status").html("Training failure");                    
//        }
//        $("#loading").hide();
//    });            
//}

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
            $("#test_images").html(`<img src="/images/${image_name}.jpg?ver=${ver++}" width="160" alt="${image_name}.jpg" /><span>${image_name}</span>`);
        }
        else {
            $("#test_status").html("Downloading failure");
        }
        $("#loading").hide();
    });
}

//function startPredictingImages() {
//    $("#loading").show();
//    $.get("/api/images/onnx/", function (data, status) {

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
    $.get("/api/images/yolo/" + imageName + ".jpg", function (data, status) {

        if (status === "success") {
            let context = canvas.getContext("2d");
            context.lineWidth = 1;
            context.font = "12pt Arial";
            context.strokeStyle = "yellow";

            let offset_x = 4;
            let offset_y = 16;
            for (let i = 0; i < data.length; i++) {
                let w = data[i].rectangle.width * yolo_ratio_x;
                let h = data[i].rectangle.height * yolo_ratio_y;
                let x = data[i].rectangle.x * yolo_ratio_x;
                let y = data[i].rectangle.y * yolo_ratio_y;

                context.strokeStyle = "yellow";
                context.rect(x, y, w, h);

                context.fillStyle = "#00000044";
                context.fillRect(x, y, w, 20);

                context.fillStyle = "yellow";
                context.fillText(`${data[i].predictedLabel} (${(data[i].probability * 100).toFixed(2)}%)`, x + offset_x, y + offset_y);
            }
            context.stroke();

            var html = '';
            for (let i = 0; i < data.length; i++) {
                html += `${data[i].predictedLabel} (${(data[i].probability * 100).toFixed(2)}%)<br />`;
            }

            $("#test_status").html("<br />Detected as: <br /><br />" + html);
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


function populateYoloImages() {
    $("#loading").show();
    $.get("/api/images/yolo/image", function (data, status) {

        if (status === "success") {

            $("#objects").empty();

            for (var i = 0; i < data.length; i++) {
                $("#objects").append(`<li class="list-inline-item p-1 list-group-item-primary">${data[i]}</li>`);
            }
        }
        $("#loading").hide();
    });    
}
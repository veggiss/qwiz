$(document).ready(function() {
    let getType = i => ["multiple_choice", "true_false"][i];
    let qfLength = () => $(".quizForm").length + 1;
    let getRoot = a => $(a).closest(".quizForm");
    let isEdit = () => model !== null;

    $("#btnAddQuizForm").click(function() {
        $("#quizField").append(getQuizForm());
    });

    $("#btnCreateQuiz").click(function() {
        submitQuizForm();
    });

    $(".alert .close").click(function() {
        $(".alert").hide();
    });

    $(document).on("change", ".questionType", function(e) {
        let ans = getRoot(this).find(".correctAnswer");
        let alt = getRoot(this).find(".alternatives");

        switch (e.currentTarget.selectedIndex) {
            case 0:
            {
                alt.show();
                ans.empty();

                alt.find("input").each(function() {
                    $(this).attr("required", true);
                });

                ["A", "B", "C", "D"].forEach(e => {
                    ans.append($("<option></option>").text(e));
                });

                break;
            }
            case 1:
            {
                alt.hide();
                ans.empty();

                alt.find("input").each(function() {
                    $(this).attr("required", false);
                });

                [{ text: "True", value: true }, { text: "False", value: false }].forEach(e => {
                    ans.append($("<option></option>").attr("value", e.value).text(e.text));
                });

                break;
            }
            default:
            {
                console.error("Not a valid question type!");
            }
        }
    });

    $(document).on("click",
    "button.removeQuizForm",
    function() {
        getRoot(this).remove();
        updateQuizForms();
    });

    $(document).on("click",
    "button.collapseQuizForm",
    function() {
        getRoot(this).find(".collapse").slideToggle("slow", "swing");
    });

    // TODO: Here is es7's wait/async preferred, but that requires transpiling
    $(document).on("change",
    "input.questionImg", function() {
        // Es7 ->
        // let path = await uploadImage(this.files[0]).catch(e => console.log(e));
        // let questionImgPrev = getRoot(this).find(".questionImgPrev");
        // questionImgPrev.attr('src', path);
            
        uploadImage(this.files[0]).then(path => {
            getRoot(this).find(".questionImgPrev").attr('src', path);
        }).catch(e => console.log(e));
    });

    $("#quizImg").change(function() {
        // let path = await uploadImage(this.files[0]).catch(e => console.log(e));
        // $("#quizImgPrev").attr('src', path);

        uploadImage(this.files[0]).then(path => {
            $("#quizImgPrev").attr('src', path);
        }).catch(e => console.log(e));
    });

    function uploadImage(file) {
        let formData = new FormData();
        formData.set('image', file);

        return new Promise(function(resolve, reject) {
            axios.post('/api/uploadImage',
                formData,
                {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                }).then(function(response) {
                resolve(response.data);
            }).catch(function(err) {
                reject(err);
            });
        });
    }

    function updateQuizForms() {
        $(".collapseQuizForm").each(function(i) {
            this.innerHTML = "Question " + (i + 1);
        });
    }

    function submitQuizForm() {
        if (qfLength() > 1 && $("#quizForm")[0].checkValidity()) {
            let api = '/api/create';
            let quizForm = {
                topic: $("#quizTitle").val(),
                category: $("#quizCategory").val(),
                description: $("#quizDescription").val(),
                imagePath: $("#quizImgPrev").attr('src'),
                difficulty: $("#quizDifficulty").children("option:selected").val(),
                questions: []
            };

            $(".quizForm").each(function(i) {
                let type = getType([$(this).find(".questionType")[0].selectedIndex]);
                let a = $(this).find(".alternative");
                let alt = type === "multiple_choice" ? JSON.stringify([a[0].value, a[1].value, a[2].value, a[3].value]) : null;

                let question = {
                    correctAnswer: $(this).find(".correctAnswer").children("option:selected").val(),
                    questionText: $(this).find(".questionText")[0].value,
                    questionType: type,
                    imagePath: $(this).find(".questionImgPrev").attr('src'),
                    alternatives: alt,
                    difficulty: $(this).find(".questionDifficulty").children("option:selected").val().toLowerCase()
                };

                quizForm.questions.push(question);
            });

            if (isEdit()) {
                quizForm.id = model.id;
                quizForm.ownerId = model.ownerId;
                api = '/api/update';
            }

            axios.post(api, JSON.stringify(quizForm), {
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function(response) {
                if (response.status === 200) {
                    window.location.href = '/Profile';
                }
            });
        } else {
            $(".alert").show().delay(2000).fadeOut();
        }
    }

    function writeQuizEdit() {
        if (isEdit()) {
            let quizForm = model;
            $("#btnCreateQuiz").text("Update Quiz");
            $("#quizTitle").val(quizForm.topic);
            $("#quizCategory").val(quizForm.category);
            $("#quizDescription").val(quizForm.description);
            $("#quizImgPrev").attr("src", quizForm.imagePath);

            quizForm.questions.forEach(question => {
                let dom = $(getQuizForm());
                let altDom = $(dom).find(".alternative");

                $(dom).find(".questionText").val(question.questionText);
                $(dom).find(".questionDifficulty").val(question.difficulty);
                $(dom).find(".questionImgPrev").attr("src", question.imagePath);

                let alternatives = JSON.parse(question.alternatives);
                if (alternatives != null) {
                    alternatives.forEach((e, i) => {
                        $(altDom[i]).val(e);
                    });
                }

                $("#quizField").append(dom);

                $(dom).find(".questionType").val(question.questionType);
                $(dom).find(".questionType").trigger("change");
                $(dom).find(".correctAnswer").val(question.correctAnswer);
            });

            $("#deleteDiv").show();
            $("#btnDeleteQuiz").click(function() {
                axios.delete('/api/delete?id=' + quizForm.id).then(function(response) {
                    if (response.status === 200) {
                        window.location.href = '/Profile';
                    }
                });
            });
        }
    }

    function getQuizForm() {
        return `
                <div class="quizForm"><hr>
                    <button type="button" class="btn btn-light collapseQuizForm" style="width:100%">Question ${qfLength()}</button>
                    <div class="collapse">
                        <button type="button" class="close removeQuizForm" aria-label="Close">
                            <span aria-hidden="true" title="Remove Question" style="color:#dc3545;">&times;</span>
                        </button>
                        <label>Question Text</label>
                        <textarea class="border rounded bg-white questionText" required style="margin-bottom: 15px; padding: 5px; font-size: 30px; width: 100%; text-align: center;"></textarea>
                        
                        <label>Question Image</label>
                        <input type="file" class="btn btn-primary questionImg" style="margin: 5px; width: 98%" value="Add image"/>
                        <img style="display:block; margin:auto;" class="questionImgPrev" width="500"/>
                        
                        <div class="form-group">
                            <label>Question Difficulty</label>
                            <select class="form-control questionDifficulty">
                                <option value="easy">Easy</option>
                                <option value="medium">Medium</option>
                                <option value="hard">Hard</option>
                            </select>
                        </div>
                        
                        <div class="form-group">
                            <label>Question Type</label>
                            <select class="form-control questionType">
                                <option value="multiple_choice">Multiple Choice</option>
                                <option value="true_false">True or False</option>
                            </select>
                        </div>
                        
                        <div class="form-group">
                            <label>Correct Answer</label>
                            <select class="form-control correctAnswer">
                                <option value="A">A</option>
                                <option value="B">B</option>
                                <option value="C">C</option>
                                <option value="D">D</option>
                            </select>
                        </div>
                        
                        <div class="form-row alternatives">
                            <div class="form-group col-md-6">
                                <input type="text" class="form-control text-center alternative" placeholder="Alternative A" required>
                            </div>
                            <div class="form-group col-md-6">
                                <input type="text" class="form-control text-center alternative" placeholder="Alternative B" required>
                            </div>
                            <div class="form-group col-md-6">
                                <input type="text" class="form-control text-center alternative" placeholder="Alternative C" required>
                            </div>
                            <div class="form-group col-md-6">
                                <input type="text" class="form-control text-center alternative" placeholder="Alternative D" required>
                            </div>
                        </div>
                    </div>
                </div>`;
    }

    writeQuizEdit();
});
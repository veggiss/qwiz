$(document).ready(function() {
    new Vue({
        el: '#quizField',
        data: {
            isEdit: model !== null,
            question: {
                "questionType": "multiple_choice",
                "questionText": "",
                "correctAnswer": "",
                "correctAlternative": "A",
                "difficulty": "easy",
                "imagePath": "",
                "alternatives": ["", "", "", ""]
            },
            questions: []
        },
        mounted: function() {
            $("#btnAddQuestion").click(() => this.addQuestion());
            $("#btnCreateQuiz").click(() => this.submitQuiz());
            $("#quizImg").change(e => this.changeQuizImg(e));
            $(".alert .close").click(() => $(".alert").hide());
        },
        methods: {
            onCollapse: function(e) {
                $(e.target).next().slideToggle("slow", "swing");
            },
            addQuestion: function() {
                this.questions.push(this.question);
            },
            submitQuiz: function() {
                if (this.questions.length > 0 && $("#quizForm")[0].checkValidity()) {
                    let api = '/api/create';
                    let quiz = {
                        topic: $("#quizTitle").val(),
                        difficulty: $("#quizDifficulty").find(":selected").val(),
                        category: $("#quizCategory").find(":selected").val(),
                        imagePath: $("#quizImgPrev").attr('src'),
                        description: $("#quizDescription").val(),
                        questions: this.questionsToJson()
                    };

                    if (this.isEdit) {
                        quiz.id = model.id;
                        quiz.ownerId = model.ownerId;
                        api = '/api/update';
                    }
                    
                    axios.post(api, JSON.stringify(quiz), {
                        headers: {
                            'Content-Type': 'application/json'
                        }
                    }).then(function(response) {
                        if (response.status === 200) {
                            window.location.href = '/Profile';
                        }
                    }).catch(function(e) {
                        if (e.response.status === 400)
                            util.openModal(e.response.data);
                        
                        console.log(e.response.data);
                    });
                } else {
                    $(".alert").show().delay(2000).fadeOut();
                }
            },
            onChange: function(type, i, e) {
                let question = $.extend(true, {}, this.questions[i]);
                
                switch (type) {
                    case 'text':
                        question.questionText = e.target.value;
                    break;
                    case 'difficulty':
                        question.difficulty = e.target.value;
                    break;
                    case 'type':
                        let questionType = $(e.target).find(":selected").val();
                        question.questionType = questionType;
                        if (questionType === 'multiple_choice') {
                            question.correctAlternative = 'A';
                        } else if(questionType === 'true_false') {
                            question.correctAlternative = 'T';
                        }
                    break;
                    case 'answer':
                        question.correctAlternative = e.target.value;
                    break;
                    case 'image':
                        this.uploadImage(e.target.files[0])
                            .then(path => question.imagePath = path)
                            .catch(e => {
                                if (e.response.status === 400)
                                    util.openModal(e.response.data);
                            });
                    break;
                    case 'alternative':
                        let altIndex = parseInt($(e.target).attr("index"));
                        question.alternatives[altIndex] = e.target.value;
                    break;
                }

                this.$set(this.questions, i, question);
            },
            changeQuizImg: function(e) {
                this.uploadImage(e.target.files[0])
                    .then(path => $("#quizImgPrev").attr("src", path))
                    .catch(e => {
                        if (e.response.status === 400)
                            util.openModal(e.response.data);
                    });
            },
            uploadImage: function(file) {
                let formData = new FormData();
                formData.set('image', file);
                
                return new Promise(function(resolve, reject) {
                    axios.post('/api/uploadImage', formData, {
                        headers: {
                            'Content-Type': 'multipart/form-data'
                        }
                    }).then(function(response) {
                        resolve(response.data);
                    }).catch(function(err) {
                        reject(err);
                    });
                });
            },
            questionsToJson: function() {
                let questions = [];
                //TODO: This should be done on the server instead
                this.questions.forEach(e => {
                    let q = $.extend(true, {}, e);
                    switch (q.correctAlternative) {
                        case 'A':
                            q.correctAnswer = q.alternatives[0];
                        break;
                        case 'B':
                            q.correctAnswer = q.alternatives[1];
                        break;
                        case 'C':
                            q.correctAnswer = q.alternatives[2];
                        break;
                        case 'D':
                            q.correctAnswer = q.alternatives[3];
                        break;
                        case 'T':
                            q.correctAnswer = 'True';
                        break;
                        case 'F':
                            q.correctAnswer = 'False';
                        break;
                    }

                    q.alternatives = q.questionType === "multiple_choice" ? JSON.stringify(q.alternatives) : null;
                    
                    questions.push(q);
                });
                
                return questions;
            }
        }
    });
});
// QuestionType = type;
// QuestionText = text;
// CorrectAnswer = answer;
// CorrectAlternative = alternative;
// Difficulty = difficulty;
// ImagePath = imagePath;
// Alternatives = alt;
//$(document).ready(function() {
//    let getType = i => ["multiple_choice", "true_false"][i];
//    let qfLength = () => $(".quizForm").length + 1;
//    let getRoot = a => $(a).closest(".quizForm");
//    let isEdit = () => model !== null;
//
//    $("#btnAddQuizForm").click(function() {
//        $("#quizField").append(getQuizForm());
//    });
//
//    $("#btnCreateQuiz").click(function() {
//        submitQuizForm();
//    });
//
//    $(".alert .close").click(function() {
//        $(".alert").hide();
//    });
//
//    $(document).on("change", ".questionType", function(e) {
//        let ans = getRoot(this).find(".correctAlternative");
//        let alt = getRoot(this).find(".alternatives");
//        let selectedIndex = e.currentTarget.selectedIndex;
//        if (selectedIndex === 0 ) {
//            alt.show();
//            ans.empty();
//
//            alt.find("input").each(function() {
//                $(this).attr("required", true);
//            });
//
//            ["A", "B", "C", "D"].forEach(e => {
//                ans.append($("<option></option>").text(e));
//            });
//        } else {
//            alt.hide();
//            ans.empty();
//
//            alt.find("input").each(function() {
//                $(this).attr("required", false);
//            });
//
//            [{ text: "True", value: true }, { text: "False", value: false }].forEach(e => {
//                ans.append($("<option></option>").attr("value", e.value).text(e.text));
//            });
//        }
//    });
//
//    $(document).on("click",
//    "button.removeQuizForm",
//    function() {
//        getRoot(this).remove();
//        updateQuizForms();
//    });
//
//    $(document).on("click",
//    "button.collapseQuizForm",
//    function() {
//        getRoot(this).find(".collapse").slideToggle("slow", "swing");
//    });
//
//    $(document).on("change",
//    "input.questionImg", function() {            
//        uploadImage(this.files[0]).then(path => {
//            getRoot(this).find(".questionImgPrev").attr('src', path);
//        }).catch(e => console.log(e));
//    });
//
//    $("#quizImg").change(function() {
//        uploadImage(this.files[0]).then(path => {
//            $("#quizImgPrev").attr('src', path);
//        }).catch(e => console.log(e));
//    });
//
//    function uploadImageuploadImage(file) {
//        let formData = new FormData();
//        formData.set('image', file);
//
//        return new Promise(function(resolve, reject) {
//            axios.post('/api/uploadImage',
//                formData,
//                {
//                    headers: {
//                        'Content-Type': 'multipart/form-data'
//                    }
//                }).then(function(response) {
//                resolve(response.data);
//            }).catch(function(err) {
//                reject(err);
//            });
//        });
//    }
//
//    function updateQuizForms() {
//        $(".collapseQuizForm").each(function(i) {
//            this.innerHTML = "Question " + (i + 1);
//        });
//    }
//
//    function submitQuizForm() {
//        if (qfLength() > 1 && $("#quizForm")[0].checkValidity()) {
//            let api = '/api/create';
//            let quizForm = {
//                topic: $("#quizTitle").val(),
//                category: $("#quizCategory").val(),
//                description: $("#quizDescription").val(),
//                imagePath: $("#quizImgPrev").attr('src'),
//                difficulty: $("#quizDifficulty").children("option:selected").val(),
//                questions: []
//            };
//
//            $(".quizForm").each(function(i) {
//                let type = getType([$(this).find(".questionType")[0].selectedIndex]);
//                let a = $(this).find(".alternative");
//                let alt = type === "multiple_choice" ? JSON.stringify([a[0].value, a[1].value, a[2].value, a[3].value]) : null;
//                let correctAlternative = $(this).find(".correctAlternative").children("option:selected")[0].value;
//                let correctAnswer = type === "true_false" ? correctAlternative : $(this).find(`[name=${correctAlternative}]`)[0].value;
//
//                let question = {
//                    correctAnswer: correctAnswer,
//                    correctAlternative: correctAlternative,
//                    questionText: $(this).find(".questionText")[0].value,
//                    questionType: type,
//                    imagePath: $(this).find(".questionImgPrev").attr('src'),
//                    alternatives: alt,
//                    difficulty: $(this).find(".questionDifficulty").children("option:selected").val().toLowerCase()
//                };
//
//                quizForm.questions.push(question);
//            });
//
//            if (isEdit()) {
//                quizForm.id = model.id;
//                quizForm.ownerId = model.ownerId;
//                api = '/api/update';
//            }
//
//            axios.post(api, JSON.stringify(quizForm), {
//                headers: {
//                    'Content-Type': 'application/json'
//                }
//            }).then(function(response) {
//                if (response.status === 200) {
//                    window.location.href = '/Profile';
//                }
//                
//                console.log("testingsss", response);
//            }).catch(function(e) {
//                util.openModal(e.response.data);
//            });
//        } else {
//        $(".alert").hide();
//    });
//            $(".alert").show().delay(2000).fadeOut();
//        }
//    }
//
//    function writeQuizEdit() {
//        if (isEdit()) {
//            let quizForm = model;
//            $("#btnCreateQuiz").text("Update Quiz");
//            $("#quizTitle").val(quizForm.topic);
//            $("#quizCategory").val(quizForm.category);
//            $("#quizDescription").val(quizForm.description);
//            $("#quizImgPrev").attr("src", quizForm.imagePath);
//
//            quizForm.questions.forEach(question => {
//                let dom = $(getQuizForm());
//                let altDom = $(dom).find(".alternative");
//
//                $(dom).find(".questionText").val(question.questionText);
//                $(dom).find(".questionDifficulty").val(question.difficulty);
//                $(dom).find(".questionImgPrev").attr("src", question.imagePath);
//
//                let alternatives = JSON.parse(question.alternatives);
//                if (alternatives != null) {
//                    alternatives.forEach((e, i) => {
//                        $(altDom[i]).val(e);
//                    });
//                }
//
//                $("#quizField").append(dom);
//
//                $(dom).find(".questionType").val(question.questionType);
//                $(dom).find(".questionType").trigger("change");
//                $(dom).find(".correctAlternative").val(question.correctAlternative);
//            });
//
//            $("#deleteDiv").show();
//            $("#btnDeleteQuiz").click(function() {
//                axios.delete('/api/delete?id=' + quizForm.id).then(function(response) {
//                    if (response.status === 200) {
//                        window.location.href = '/Profile';
//                    }
//                });
//            });
//        }
//    }
//
//    function getQuizForm() {
//        return `
//                <div class="quizForm"><hr>
//                    <button type="button" class="btn btn-light collapseQuizForm" style="width:100%">Question ${qfLength()}</button>
//                    <div class="collapse">
//                        <button type="button" class="close removeQuizForm" aria-label="Close">
//                            <span aria-hidden="true" title="Remove Question" style="color:#dc3545;">&times;</span>
//                        </button>
//                        <label>Question Text</label>
//                        <textarea class="border rounded bg-white questionText" maxlength="128" required style="margin-bottom: 15px; padding: 5px; font-size: 30px; width: 100%; text-align: center;"></textarea>
//                        
//                        <label>Question Image</label>
//                        <input type="file" class="btn btn-primary questionImg" style="margin: 5px; width: 98%" value="Add image"/>
//                        <img style="display:block; margin:auto;" class="questionImgPrev" width="500"/>
//                        
//                        <div class="form-group">
//                            <label>Question Difficulty</label>
//                            <select class="form-control questionDifficulty">
//                                <option value="easy">Easy</option>
//                                <option value="medium">Medium</option>
//                                <option value="hard">Hard</option>
//                            </select>
//                        </div>
//                        
//                        <div class="form-group">
//                            <label>Question Type</label>
//                            <select class="form-control questionType">
//                                <option value="multiple_choice">Multiple Choice</option>
//                                <option value="true_false">True or False</option>
//                            </select>
//                        </div>
//                        
//                        <div class="form-group">
//                            <label>Correct Answer</label>
//                            <select class="form-control correctAlternative">
//                                <option value="A">A</option>
//                                <option value="B">B</option>
//                                <option value="C">C</option>
//                                <option value="D">D</option>
//                            </select>
//                        </div>
//                        
//                        <div class="form-row alternatives">
//                            <div class="form-group col-md-6">
//                                <input type="text" name="A" class="form-control text-center alternative" placeholder="Alternative A" required>
//                            </div>
//                            <div class="form-group col-md-6">
//                                <input type="text" name="B" class="form-control text-center alternative" placeholder="Alternative B" required>
//                            </div>
//                            <div class="form-group col-md-6">
//                                <input type="text" name="C" class="form-control text-center alternative" placeholder="Alternative C" required>
//                            </div>
//                            <div class="form-group col-md-6">
//                                <input type="text" name="D" class="form-control text-center alternative" placeholder="Alternative D" required>
//                            </div>
//                        </div>
//                    </div>
//                </div>`;
//    }
//
//    writeQuizEdit();
//});
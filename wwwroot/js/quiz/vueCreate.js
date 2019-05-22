Vue.component('questionForm', {
    props: ['question', 'index'],
    template:
`
<div><hr>
    <button type="button" class="btn btn-outline-light w-100" v-on:click="$emit('collapse', $event)">Question {{ index + 1 }}</button>
    <div class="collapse">
        <button type="button" class="close" aria-label="Close" v-on:click="$emit('remove', index)">
            <span aria-hidden="true" title="Remove Question" style="color:#dc3545;">&times;</span>
        </button>
        
        <label class="mt-3">Question Text</label>
        <textarea class="form-control text-center" maxlength="128" style="font-size: 22px;"
        v-on:change="$emit('change', 'text', index, $event)" :value="question.questionText" required></textarea>
        
        <div class="form-group w-100">
            <label>Question Image</label>
            <input type="file" class="btn w-100" v-on:change="$emit('change', 'image', index, $event)"/>
            <img class="py-2 d-block m-auto" width="500" :src="question.imagePath"/>
        </div>
        
        <div class="form-row">
            <div class="col form-group">
                <label class="small">Difficulty</label>
                <select class="form-control" v-on:change="$emit('change', 'difficulty', index, $event)">
                    <option :selected="question.difficulty === 'easy'"   value="easy"  >Easy</option>
                    <option :selected="question.difficulty === 'medium'" value="medium">Medium</option>
                    <option :selected="question.difficulty === 'hard'"   value="hard"  >Hard</option>
                </select>
            </div>
        
            <div class="col form-group">
                <label class="small">Type</label>
                <select class="form-control" v-on:change="$emit('change', 'type', index, $event)">
                    <option value="multiple_choice">Multiple Choice</option>
                    <option value="true_false">True or False</option>
                </select>
            </div>
            
            <div class="col form-group">
                <label class="small">Correct Answer</label>
                <select class="form-control" v-if="question.questionType == 'multiple_choice'" v-on:change="$emit('change', 'answer', index, $event)">
                    <option :selected="question.correctAlternative === 'A'" value="A">A</option>
                    <option :selected="question.correctAlternative === 'B'" value="B">B</option>
                    <option :selected="question.correctAlternative === 'C'" value="C">C</option>
                    <option :selected="question.correctAlternative === 'D'" value="D">D</option>
                </select>
                
                <select class="form-control" v-else-if="question.questionType == 'true_false'" v-on:change="$emit('change', 'answer', index, $event)">
                    <option :selected="question.correctAlternative === 'T'" value="T">True</option>
                    <option :selected="question.correctAlternative === 'F'" value="F">False</option>
                </select>
            </div>
        </div>
        
        <div class="form-row" v-if="question.questionType == 'multiple_choice'" v-on:change="$emit('change', 'alternative', index, $event)">
            <div class="form-group col-md-6">
                <input type="text" index="0" class="form-control text-center" placeholder="Alternative A" maxlength="64" :value="question.alternatives[0]" required>
            </div>
            <div class="form-group col-md-6">
                <input type="text" index="1" class="form-control text-center" placeholder="Alternative B" maxlength="64" :value="question.alternatives[1]" required>
            </div>
            <div class="form-group col-md-6">
                <input type="text" index="2" class="form-control text-center" placeholder="Alternative C" maxlength="64" :value="question.alternatives[2]" required>
            </div>
            <div class="form-group col-md-6">
                <input type="text" index="3" class="form-control text-center" placeholder="Alternative D" maxlength="64" :value="question.alternatives[3]" required>
            </div>
        </div>
    </div>
</div>
`
});
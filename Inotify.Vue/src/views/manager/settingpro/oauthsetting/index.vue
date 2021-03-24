<template>
<div class='app-container'>
    <el-card>
        <template #header>
            <div class="card-header">
                <span>重置密码</span>
            </div>
        </template>
        <el-form ref="oauthForm" :model="oauthForm" label-width="20%" label-position='right' :rules="oauthRules">
            <el-form-item label="用户名">
                <el-input v-model="oauthForm.username" :disabled="true"></el-input>
            </el-form-item>
            <el-form-item prop="newpassword" label='新密码'>
                <el-input ref="password" name="password" v-model='oauthForm.newpassword' placeholder="新密码"></el-input>
            </el-form-item>
            <el-form-item prop="newpassword2" label='确认密码'>
                <el-input ref="password2" name="password2" v-model="oauthForm.newpassword2" placeholder="确认密码"></el-input>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="submitForm('oauthForm')">重置</el-button>
            </el-form-item>
        </el-form>
    </el-card>
</div>
</template>

<script>
import {
    resetPassword
} from '@/api/user'

export default {
    data() {
        var validatePassword = (rule, value, callback) => {
            if (value.length < 6) {
                callback(new Error('密码长度必须大于6'))
            } else {

                callback()

            }
        }

        var validatePassword2 = (rule, value, callback) => {
            if (value.length < 6) {
                callback(new Error('密码长度必须大于6'))
            } else {
                if (this.oauthForm.newpassword !== this.oauthForm.newpassword2) {
                    callback(new Error('密码不一致'))
                } else {
                    callback()
                }
            }
        }
        return {
            oauthForm: {
                username: '',
                newpassword: '',
                newpassword2: ''
            },
            oauthRules: {
                newpassword: [{
                    required: true,
                    trigger: 'blur',
                    validator: validatePassword
                }],
                newpassword2: [{
                    required: true,
                    trigger: 'blur',
                    validator: validatePassword2
                }]
            }
        }
    },
    created() {
        this.fetchData()
    },
    methods: {
        fetchData() {

            this.oauthForm.username = this.$store.getters.name;
        },
        submitForm(formName) {
            this.$refs[formName].validate((valid) => {

                resetPassword(this.oauthForm.newpassword2).then((response) => {
                    if (response.code == 200) {
                        this.$message({
                            message: '重置成功',
                            type: 'success'
                        });
                    } else {
                        this.$message.error('重置失败');
                    }
                });
            });
        },
    }
}
</script>

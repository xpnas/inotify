<template>
<div class="app-container">
    <el-card>
        <template #header>
            <div class="card-header">
                <span>JWT验证</span>
            </div>
        </template>
        <el-form ref="settingForm" :model="jwt" label-width="20%" label-position="right" :rules="jwtRules">
            <el-form-item prop="clockSkew" label="ClockSkew">
                <el-input v-model="jwt.clockSkew" placeholder="ClockSkew">></el-input>
            </el-form-item>
            <el-form-item prop="audience" label="Audience">
                <el-input v-model="jwt.audience" placeholder="Audience"></el-input>
            </el-form-item>
            <el-form-item prop="issuer" label="Issuer">
                <el-input v-model="jwt.issuer" placeholder="Issuer"></el-input>
            </el-form-item>
            <el-form-item prop="issuerSigningKey" label="IssuerSigningKey">
                <el-input type="textarea" v-model="jwt.issuerSigningKey" placeholder="IssuerSigningKey"></el-input>
            </el-form-item>
            <el-form-item prop="expiration" label="Expiration">
                <el-input v-model="jwt.expiration" placeholder="Expiration"></el-input>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="submitForm('settingForm')">确认修改</el-button>
            </el-form-item>

        </el-form>
    </el-card>
</div>
</template>

<script>
import {
    getJWT,
    setJWT
} from '@/api/systemsetting'

export default {

    data() {
        return {
            jwtRules: {
                clockSkew: [{
                    required: true,
                    message: '请输入clockSkew',
                    trigger: 'blur'
                }],
                audience: [{
                    required: true,
                    message: '请输入audience',
                    trigger: 'blur'
                }],
                issuer: [{
                    required: true,
                    message: '请输入issuer',
                    trigger: 'blur'
                }],
                issuerSigningKey: [{
                    required: true,
                    message: '请输入issuerSigningKey',
                    trigger: 'blur'
                }],
                expiration: [{
                    required: true,
                    message: '请输入expiration',
                    trigger: 'blur'
                }],
            },

            jwt: {

            }
        }
    },
    created() {
        this.fetchData()
    },
    methods: {
        fetchData() {
            getJWT().then((resposne) => {
                this.jwt = resposne.data
            });
        },
        submitForm(formName) {
            this.$refs[formName].validate((valid) => {
                this.$confirm('您修改了JWT授权，这将影响所有已登录用户导致需要重新登陆，确认修改吗', '警告', {
                    distinguishCancelAndClose: true,
                    confirmButtonText: '确定修改',
                    cancelButtonText: '放弃修改',
                    type: 'warning',
                    center: true,
                    callback: action => {
                        if (action == 'confirm') {
                            setJWT(this.jwt).then((response) => {
                                if (response.code == 200) {
                                    this.$message({
                                        message: '设置成功',
                                        type: 'success'
                                    })
                                } else {
                                    this.$message.error('设置失败');
                                }
                            })
                        }
                    }
                })
            })
        }
    }
}
</script>

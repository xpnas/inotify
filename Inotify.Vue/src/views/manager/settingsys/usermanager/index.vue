<template>
<div class="app-container">
    <div style="margin-top:10px;margin-bottom:10px;width:400px;">
        <el-input v-model='query' @change='search' width='250' placeholder="请输入内容" >
            <el-button slot="append" icon="el-icon-search"></el-button>
        </el-input>
    </div>
    <el-table  style="height:100%" v-loading='listLoading' :data='data.items' element-loading-text='加载中' border fit highlight-current-row>
        <el-table-column align='center' label='序号' width='95'>
            <template slot-scope='scope'>
                {{ scope.$index+1 }}
            </template>
        </el-table-column>
        <el-table-column label='用户名' width='110' align='center'>
            <template slot-scope='scope'>
                <span>{{ scope.row.userName }}</span>
            </template>
        </el-table-column>
        <el-table-column label='邮箱' align='center'>
            <template slot-scope='scope'>
                <span>{{ scope.row.email }}</span>
            </template>
        </el-table-column>
        <el-table-column label='创建时间' width='160' align='center'>
            <template slot-scope='scope'>
                <el-date-picker type="date"  v-model='scope.row.createTime' value-format="yyyy-MM-dd" style="width: 100%;" readonly></el-date-picker>
                <span hidden>{{ scope.row.email }}</span>
            </template>
        </el-table-column>
        <el-table-column align='center' prop='created_at' label='编辑' width='100'>
            <template slot-scope='scope'>
                <el-button-group>
                    <!-- <el-button type='primary' icon='el-icon-edit' @click='modifyuser(scope.$index, scope.row)'></el-button> -->
                    <el-button type='danger' icon='el-icon-delete' @click='deleteuser(scope.$index, scope.row)'></el-button>
                </el-button-group>
            </template>
        </el-table-column>
        <el-table-column class-name='status-col' label='激活' width='110' align='center'>
            <template slot-scope='scope'>
                <el-switch active-color='#13ce66' v-model='scope.row.active' @change='activeuser(scope.row)' inactive-color='#ff4949'></el-switch>
            </template>
        </el-table-column>
    </el-table>
    <div class="block" style="text-align: right;">
        <el-pagination @size-change="handleSizeChange" @current-change="handleCurrentChange" layout="total, sizes, prev, pager, next, jumper" :current-page="currentPage" :page-sizes="[10, 20, 50, 100]" :page-size="pageSize" :total="data.totalItems">
        </el-pagination>
    </div>
</div>
</template>

<script>
import {
    getUsers,
    deleteUser,
    activeUser
} from '@/api/systemsetting'

export default {
    data() {
        return {
            query: '',
            listLoading: false,
            currentPage: 0,
            pageSize: 10,
            data: {
                currentPage: 0,
                totalPages: 0,
                totalItems: 0,
                itemsPerPage: 10,
                items: []
            }
        }
    },
    created() {
        this.fetchData()
    },
    methods: {

        fetchData() {
            getUsers(this.query, this.currentPage, this.pageSize).then((response) => {
                this.data = response.data;
            })
        },
        handleCurrentChange(val) {
            this.currentPage = val;
            this.fetchData()
        },
        handleSizeChange(val) {
            this.pageSize = val
            this.fetchData()
        },
        search() {
            this.fetchData()
        },

        modifyuser(index, row) {

        },
        deleteuser(index, row) {

            this.$confirm('确认删除用户：' + row.userName, '警告', {
                distinguishCancelAndClose: true,
                confirmButtonText: '确定删除',
                cancelButtonText: '放弃删除',
                type: 'warning',
                center: true,
                callback: action => {
                    if (action == 'confirm') {
                        deleteUser(row.userName).then((response) => {
                            if (response.code == 200) {
                                this.$message.success(row.userName + '删除成功')
                                this.fetchData()
                            } else {
                                this.$message.error(row.userName + '删除失败')
                            }
                        })
                    }
                }
            });
        },
        activeuser(row) {
            var active = row.active;
            activeUser(row.userName, active).then((response) => {
                if (response.code == 200) {
                    this.$message.success(row.userName + (active ? '激活成功' : '禁用成功'))
                    this.fetchData()
                } else {
                    this.$message.error(row.userName + (active ? '激活失败' : '禁用成功'))
                }
            })

        }
    }
}
</script>

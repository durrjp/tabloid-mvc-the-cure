﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using System;
using System.Security.Claims;
using TabloidMVC.Models;
using TabloidMVC.Models.ViewModels;
using TabloidMVC.Repositories;

namespace TabloidMVC.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly PostRepository _postRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly TagRepository _tagRepository;
        private readonly SubRepository _subRepository;

        public PostController(IConfiguration config)
        {
            _postRepository = new PostRepository(config);
            _categoryRepository = new CategoryRepository(config);
            _tagRepository = new TagRepository(config);
            _subRepository = new SubRepository(config);
        }

        public IActionResult Index()
        {
            var posts = _postRepository.GetAllUsersPosts();
            return View(posts);
        }

        public IActionResult Details(int id)
        {
            var post = _postRepository.GetPublishedPostById(id);
            var tags = _tagRepository.GetTagsByPostId(id);
            if (post == null)
            {
                int userId = GetCurrentUserProfileId();
                post = _postRepository.GetUserPostById(id, userId);
                if (post == null)
                {
                    return NotFound();
                }
            }
            var vm = new PostCreateViewModel()
            {
                Post = post,
                Tags = tags

            };
            return View(vm);
        }

        public IActionResult Create()
        {
            var vm = new PostCreateViewModel();
            vm.CategoryOptions = _categoryRepository.GetAllCategories();
            return View(vm);
        }

        [HttpPost]
        public IActionResult Create(PostCreateViewModel vm)
        {
            try
            {
                vm.Post.CreateDateTime = DateAndTime.Now;
                vm.Post.IsApproved = true;
                vm.Post.UserProfileId = GetCurrentUserProfileId();

                _postRepository.Add(vm.Post);

                return RedirectToAction("Details", new { id = vm.Post.Id });
            }
            catch
            {
                vm.CategoryOptions = _categoryRepository.GetAllCategories();
                return View(vm);
            }
        }
        
        public ActionResult MakeSub(int authorId)
        {
            try
            {
                Sub newSub = new Sub()
                {
                    SubscriberUserProfileId = GetCurrentUserProfileId(),
                    ProviderUserProfileId = authorId,
                    BeginDateTime = DateTime.Now,
                    EndDateTime = null
                };
                _subRepository.AddSub(newSub);


                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        private int GetCurrentUserProfileId()
        {
            string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(id);
        }

        public IActionResult Delete(int id)
        {
            //int UserProfileId = GetCurrentUserProfileId();
            var post = _postRepository.GetPublishedPostById(id);
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, Post post)
        {
            try
            {
                _postRepository.DeletePost(id);

                return RedirectToAction("Index");
            }
            catch
            {
                // If something goes wrong, just keep the user on the same page so they can try again
                return View(post);
            }
        }

        public IActionResult Edit(int id)
        {
            PostCreateViewModel vm = new PostCreateViewModel();
            vm.CategoryOptions = _categoryRepository.GetAllCategories();
            vm.Post = _postRepository.GetPublishedPostById(id);

            return View(vm);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, PostCreateViewModel vm)
        {
            try
            {
                vm.CategoryOptions = _categoryRepository.GetAllCategories();
                vm.Post.UserProfileId = GetCurrentUserProfileId();
                vm.Post.Id = id;
                vm.Post.CreateDateTime = DateAndTime.Now;

                _postRepository.UpdatePost(vm.Post);

                return RedirectToAction("Details", new { id = vm.Post.Id });
            }
            catch
            {
                vm.CategoryOptions = _categoryRepository.GetAllCategories();
                return View(vm);
            }


        }

    }
}

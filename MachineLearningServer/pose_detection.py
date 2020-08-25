import cv2
import torch
from PIL import Image

import posenet

def get_pose_coordinates(img):
    model = posenet.load_model(101)
    model = model.cuda()
    output_stride = model.output_stride

    scale_factor = 1.0
    input_image, draw_image, output_scale = posenet.read_pil_img(img, scale_factor, output_stride)

    with torch.no_grad():
        input_image = torch.Tensor(input_image).cuda()

        heatmaps_result, offsets_result, displacement_fwd_result, displacement_bwd_result = model(input_image)
        pose_scores, keypoint_scores, keypoint_coords = posenet.decode_multiple_poses(
            heatmaps_result.squeeze(0),
            offsets_result.squeeze(0),
            displacement_fwd_result.squeeze(0),
            displacement_bwd_result.squeeze(0),
            output_stride=output_stride,
            max_pose_detections=10,
            min_pose_score=0.25)

    keypoint_coords *= output_scale

    # draw_image = posenet.draw_skel_and_kp(
    #    draw_image, pose_scores, keypoint_scores, keypoint_coords, min_pose_score=0.25, min_part_score=0.25)

    # cv2.imwrite('poseimage.jpg', draw_image)

    results = []

    for pi in range(len(pose_scores)):
        pose = dict()
        if pose_scores[pi] == 0.:
            break
        pose["PoseID"] = pi
        pose["PoseScore"] = pose_scores[pi]
        pose["Keypoints"] = []
        for ki, (s, c) in enumerate(zip(keypoint_scores[pi, :], keypoint_coords[pi, :, :])):
            keypoint_dict = dict()
            keypoint = {
                "Score:"   : s,
                "Position" : [c[0], c[1], 0]
            }
            keypoint_dict[posenet.PART_NAMES[ki]] = keypoint
            pose["Keypoints"].append(keypoint_dict)

        results.append(pose)
    return results


